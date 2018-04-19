using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Migrations;
using VamBooru.Models;

namespace VamBooru.Repository
{
	public class EntityFrameworkRepository : IRepository
	{
		private readonly VamBooruDbContext _context;

		public EntityFrameworkRepository(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes, DateTimeOffset now)
		{
			var user = await LoadPrivateUserAsync(login);

			if(user == null) throw new UnauthorizedAccessException("No user found for this login");

			var post = new Post
			{
				Title = title,
				Text = "",
				Author = user,
				DateCreated = now
			};

			foreach (var scene in scenes)
			{
				_context.Scenes.Add(scene);
				post.Scenes.Add(scene);
			}
			await AssignTagsAsync(post, tags);

			_context.Posts.Add(post);
			await _context.SaveChangesAsync();
			return post;
		}

		public Task<Post> LoadPostAsync(Guid id)
		{
			return _context.Posts
				.Include(p => p.Author)
				.Include(p => p.Tags)
				.ThenInclude(t => t.Tag)
				.Include(p => p.Scenes)
				.FirstOrDefaultAsync(p => p.Id == id);
		}

		public Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page, int pageSize, DateTimeOffset now)
		{
			if(page < 0) throw new ArgumentException("Page must be greater than or equal to 0");
			if(pageSize < 1) throw new ArgumentException("Page must be greater than or equal to 1");

			var baseQuery = _context.Posts
				.AsNoTracking()
				.Include(p => p.Author)
				.Include(p => p.Tags)
				.ThenInclude(t => t.Tag)
				.Include(p => p.Scenes)
				.Select(p => new Post
				{
					Id = p.Id,
					Title = p.Title,
					Author = p.Author,
					Published = p.Published,
					DateCreated = p.DateCreated,
					DatePublished = p.DatePublished,
					ImageUrl = p.ImageUrl,
					Tags = p.Tags,
					Votes = p.Votes,
					//TODO: This is only required to populate the post's image URL. We should associate it upfront instead of always loading all scenes.
					Scenes = p.Scenes
				})
				.Where(p => p.Published);

			if (since != PostedSince.Forever)
			{
				var dateTimeOffset = now.AddDays(-(int)since);
				baseQuery = sortBy == PostSortBy.Updated
					? baseQuery.Where(p => p.DatePublished >= dateTimeOffset)
					: baseQuery.Where(p => p.DateCreated >= dateTimeOffset);
			}

			switch (sortBy)
			{
				case PostSortBy.Votes:
					baseQuery = sortDirection == PostSortDirection.Down
						? baseQuery.OrderByDescending(p => p.Votes).ThenByDescending(p => p.DateCreated)
						: baseQuery.OrderBy(p => p.Votes).ThenBy(p => p.DateCreated);
					break;
				case PostSortBy.Created:
					baseQuery = sortDirection == PostSortDirection.Down
						? baseQuery.OrderByDescending(p => p.DateCreated)
						: baseQuery.OrderBy(p => p.DateCreated);
					break;
				case PostSortBy.Updated:
					baseQuery = sortDirection == PostSortDirection.Down
						? baseQuery.OrderByDescending(p => p.DatePublished)
						: baseQuery.OrderBy(p => p.DatePublished);
					break;
				default:
					throw new ArgumentException($"Unknown sort by: {sortBy}", nameof(sortBy));
			}

			return baseQuery
				.Skip(page * pageSize)
				.Take(pageSize)
				.ToArrayAsync();
		}

		public async Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now)
		{
			var user = await LoadPrivateUserAsync(login);
			var dbPost = await LoadPostAsync(Guid.Parse(post.Id));
			if(dbPost.Author.Id != user.Id) throw new UnauthorizedAccessException();

			dbPost.Title = post.Title;
			dbPost.Text = post.Text;
			if (!dbPost.Published && post.Published) dbPost.DatePublished = now;
			dbPost.Published = post.Published;
			await AssignTagsAsync(dbPost, post.Tags.Select(tag => tag.Name).ToArray());

			await _context.SaveChangesAsync();

			return dbPost;
		}

		private async Task AssignTagsAsync(Post post, string[] tags)
		{
			// Remove tags
			foreach (var tag in post.Tags.ToArray())
			{
				if (!tags.Contains(tag.Tag.Name))
					post.Tags.Remove(tag);
			}

			// Add tags
			var currentTags = post.Tags.Select(t => t.Tag.Name).ToArray();
			var newTags = tags.Where(t => !currentTags.Contains(t)).ToArray();
			foreach (var newTag in await GetOrCreateTagsAsync(newTags))
			{
				var postTag = new PostTag { Tag = newTag };
				post.Tags.Add(postTag);
				_context.PostTags.Add(postTag);
			}
		}

		private async Task<Tag[]> GetOrCreateTagsAsync(string[] tags)
		{
			if (!(tags?.Any() ?? false)) return new Tag[0];

			var dbTags = new List<Tag>();

			//TODO: This is not "thread safe", multiple users could create the same tags at the same time
			foreach (var tag in tags)
			{
				var dbTag = await _context.Tags.SingleOrDefaultAsync(t => t.Name == tag);
				if (dbTag == null)
				{
					dbTag = new Tag {Name = tag};
					_context.Tags.Add(dbTag);
				}
				dbTags.Add(dbTag);
			}

			return dbTags.ToArray();
		}

		public Task<SceneFile[]> LoadPostFilesAsync(Guid postId, bool includeBytes)
		{
			var query = _context.SceneFiles
				.Include(sf => sf.Scene)
				.Where(sf => sf.Scene.Post.Id == postId);

			query = includeBytes
				? query.Select(sf => new SceneFile {Filename = sf.Filename, Scene = sf.Scene, Bytes = sf.Bytes})
				: query.Select(sf => new SceneFile {Filename = sf.Filename, Scene = sf.Scene});

			return query.ToArrayAsync();
		}

		public async Task<UserLogin> CreateUserFromLoginAsync(string scheme, string id, string name, DateTimeOffset now)
		{
			var login = await _context.UserLogins.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == id);

			if (login != null) return login;

			var user = new User { Username = name, DateSubscribed = now };
			_context.Users.Add(user);

			login = new UserLogin { User = user, Scheme = scheme, NameIdentifier = id };
			_context.UserLogins.Add(login);

			await _context.SaveChangesAsync();
			return login;
		}

		public Task<User> LoadPrivateUserAsync(UserLoginInfo info)
		{
			return LoadPrivateUserAsync(info.Scheme, info.NameIdentifier);
		}

		public async Task<User> LoadPrivateUserAsync(string scheme, string id)
		{
			var login = await _context.UserLogins
				.Include(l => l.User)
				.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == id);

			return login?.User;
		}

		public Task<User> LoadPublicUserAsync(Guid userId)
		{
			return _context.Users.FindAsync(userId);
		}

		public async Task<User> UpdateUserAsync(UserLoginInfo login, UserViewModel user)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");

			dbUser.Username = user.Username;

			await _context.SaveChangesAsync();

			return dbUser;
		}

		public Task<Tag[]> SearchTags(string q)
		{
			//TODO: We should use full text search here
			return _context.Tags
				.Where(t => t.Name.Contains(q))
				.ToArrayAsync();
		}

		public async Task<UserPostVote> GetVoteAsync(UserLoginInfo login, Guid postId)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			return await _context.UserPostVotes.Where(upv => upv.User == dbUser && upv.PostId == postId).FirstOrDefaultAsync();
		}

		public async Task<int> VoteAsync(UserLoginInfo login, Guid postId, int votes)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			var dbVote = await _context.UserPostVotes.Where(upv => upv.User == dbUser && upv.PostId == postId).FirstOrDefaultAsync();
			int difference;
			if (dbVote == null)
			{
				difference = votes;
				if (votes != 0)
				{
					dbVote = new UserPostVote {PostId = postId, User = dbUser, Votes = votes};
					_context.UserPostVotes.Add(dbVote);
				}
			}
			else
			{
				difference = votes - dbVote.Votes;
				if (votes == 0)
					_context.UserPostVotes.Remove(dbVote);
				else
					dbVote.Votes = votes;
			}

			if (difference != 0)
				//TODO: It is theoritically possible that the same user sends multiple upvotes VERY fast and create a few fake votes.
				await _context.Database.ExecuteSqlCommandAsync($"UPDATE \"Posts\" SET \"Votes\" = \"Votes\" + {difference} WHERE \"Id\" = {postId:B}");

			await _context.SaveChangesAsync();

			return difference;
		}
	}
}

