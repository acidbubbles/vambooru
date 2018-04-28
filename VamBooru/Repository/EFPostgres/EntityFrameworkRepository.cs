using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkRepository : IRepository
	{
		private readonly VamBooruDbContext _context;

		public EntityFrameworkRepository(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes,
			PostFile[] files, string thumbnailUrn, DateTimeOffset now)
		{
			var user = await LoadPrivateUserAsync(login);

			if(user == null) throw new UnauthorizedAccessException("No user found for this login");

			var post = new Post
			{
				Title = title,
				Text = "",
				Author = user,
				ThumbnailUrn = thumbnailUrn,
				DateCreated = now
			};

			foreach (var scene in scenes)
			{
				if(scene.Id != Guid.Empty) throw new UnauthorizedAccessException("Cannot assign an existing scene to a new post");
				_context.Scenes.Add(scene);
				post.Scenes.Add(scene);
			}
			await AssignTagsAsync(post, tags);

			foreach (var file in files)
			{
				if(file.Id != 0) throw new UnauthorizedAccessException("Cannot assign an existing file to a new post");
				_context.PostFiles.Add(file);
				post.PostFiles.Add(file);
			}

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

		public Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page, int pageSize, string[] tags, string author, string text, DateTimeOffset now)
		{
			if(page < 0) throw new ArgumentException("Page must be greater than or equal to 0");
			if(pageSize < 1) throw new ArgumentException("Page must be greater than or equal to 1");

			var baseQuery = _context.Posts
				.AsNoTracking()
				.Include(p => p.Author)
				.Include(p => p.Tags)
				.ThenInclude(t => t.Tag)
				.Where(p => p.Published);

			if (since != PostedSince.Forever)
			{
				var dateTimeOffset = now.AddDays(-(int)since);
				baseQuery = sortBy == PostSortBy.Updated
					? baseQuery.Where(p => p.DatePublished >= dateTimeOffset)
					: baseQuery.Where(p => p.DateCreated >= dateTimeOffset);
			}

			if (!string.IsNullOrWhiteSpace(author))
			{
				baseQuery = baseQuery.Where(p => p.Author.Username == author);
			}

			if (!string.IsNullOrWhiteSpace(text))
			{
				//TODO: Replace with ts_vector full text search when available
				baseQuery = baseQuery.Where(p => p.Text.Contains(text) || p.Title.Contains(text));
			}

			if (tags != null && tags.Length > 0)
			{
				baseQuery = baseQuery.Where(p => tags.All(t => p.Tags.Any(pt => pt.Tag.Name == t)));
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
				.Select(p => new Post
				{
					Id = p.Id,
					Title = p.Title,
					Author = p.Author,
					Published = p.Published,
					DateCreated = p.DateCreated,
					DatePublished = p.DatePublished,
					ThumbnailUrn = p.ThumbnailUrn,
					Tags = p.Tags,
					Votes = p.Votes,
				})
				.ToArrayAsync();
		}

		public async Task<Post[]> BrowseMyPostsAsync(UserLoginInfo login)
		{
			var user = await LoadPrivateUserAsync(login);

			return await _context.Posts
				.AsNoTracking()
				.Include(p => p.Author)
				.Include(p => p.Tags)
				.ThenInclude(t => t.Tag)
				.Where(p => p.Author == user)
				.OrderByDescending(p => p.DatePublished)
				.Select(p => new Post
				{
					Id = p.Id,
					Title = p.Title,
					Author = p.Author,
					Published = p.Published,
					DateCreated = p.DateCreated,
					DatePublished = p.DatePublished,
					ThumbnailUrn = p.ThumbnailUrn,
					Tags = p.Tags,
					Votes = p.Votes,
				})
				.ToArrayAsync();
		}

		public async Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now)
		{
			var user = await LoadPrivateUserAsync(login);
			var dbPost = await LoadPostAsync(Guid.Parse(post.Id));
			var wasPublished = dbPost.Published;
			if(dbPost.Author.Id != user.Id) throw new UnauthorizedAccessException();

			dbPost.Title = post.Title;
			dbPost.Text = post.Text;
			if (!wasPublished && post.Published)
				dbPost.DatePublished = now;
			dbPost.Published = post.Published;
			var tagsAssignationResult = await AssignTagsAsync(dbPost, post.Tags.Select(tag => tag.Name).ToArray());

			await _context.SaveChangesAsync();

			if (!wasPublished && post.Published)
			{
				// Just published: increment everything
				await ChangeTagsPostsCount(dbPost.Tags.Select(tag => tag.Tag).ToArray(), 1);
			}
			else if (wasPublished && !post.Published)
			{
				// Unpublished: decrement everything
				await ChangeTagsPostsCount(dbPost.Tags.Select(tag => tag.Tag).ToArray(), -1);
			}
			else if (post.Published)
			{
				// Changed
				await ChangeTagsPostsCount(tagsAssignationResult.RemovedTags, -1);
				await ChangeTagsPostsCount(tagsAssignationResult.AddedTags, 1);
			}

			return dbPost;
		}

		private async Task ChangeTagsPostsCount(ICollection<Tag> tags, int increment)
		{
			if (tags.Count <= 0)
				return;

			var ids = tags.Select(tag => tag.Id).ToArray();
			await _context.Database.ExecuteSqlCommandAsync(
				"UPDATE \"Tags\" SET \"PostsCount\" = \"PostsCount\" + :increment WHERE \"Id\" = ANY(:ids)",
				new NpgsqlParameter("increment", DbType.Int32) { Value = increment },
				// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
				new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) { Value = ids }
			);
		}

		private async Task<TagsAssignationResult> AssignTagsAsync(Post post, string[] tags)
		{
			var result = new TagsAssignationResult();

			// Remove tags
			foreach (var tag in post.Tags.ToArray())
			{
				if (!tags.Contains(tag.Tag.Name))
				{
					post.Tags.Remove(tag);
					result.RemovedTags.Add(tag.Tag);
				}
			}

			// Add tags
			var currentTags = post.Tags.Select(t => t.Tag.Name).ToArray();
			var newTags = tags.Where(t => !currentTags.Contains(t)).ToArray();
			foreach (var newTag in await GetOrCreateTagsAsync(newTags))
			{
				var postTag = new PostTag { Tag = newTag };
				post.Tags.Add(postTag);
				_context.PostTags.Add(postTag);
				result.AddedTags.Add(newTag);
			}

			return result;
		}

		private class TagsAssignationResult
		{
			public ICollection<Tag> RemovedTags { get; } = new List<Tag>();
			public ICollection<Tag> AddedTags { get; } = new List<Tag>();
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

		public Task<PostFile[]> LoadPostFilesAsync(Guid postId)
		{
			return _context.PostFiles
				.Where(sf => sf.Post.Id == postId)
				.ToArrayAsync();
		}

		public Task<PostFile> LoadPostFileAsync(Guid postId, string urn)
		{
			//TODO: Not unit tested
			return _context.PostFiles
				.SingleOrDefaultAsync(sf => sf.Urn == urn && sf.Post.Id == postId);
		}

		public async Task<LoadOrCreateUserFromLoginResult> LoadOrCreateUserFromLoginAsync(string scheme, string nameIdentifier, string username, DateTimeOffset now)
		{
			var login = await _context.UserLogins
				.Include(ul => ul.User)
				.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == nameIdentifier);

			if (login != null) return new LoadOrCreateUserFromLoginResult
			{
				Login = login,
				Result = LoadOrCreateUserFromLoginResultTypes.ExistingUser
			};

			var user = new User { Username = username, DateSubscribed = now };
			_context.Users.Add(user);

			login = new UserLogin { User = user, Scheme = scheme, NameIdentifier = nameIdentifier };
			_context.UserLogins.Add(login);

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException exc)
			{
				var inner = exc.InnerException as NpgsqlException;
				if (inner?.Message?.Contains("IX_Users_Username") ?? false)
					throw new UsernameConflictException();
			}

			return new LoadOrCreateUserFromLoginResult
			{
				Login = login,
				Result = LoadOrCreateUserFromLoginResultTypes.NewUser
			};
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

		public Task<User> LoadPublicUserAsync(string username)
		{
			return _context.Users.FirstOrDefaultAsync(u => u.Username == username);
		}

		public async Task<User> UpdateUserAsync(UserLoginInfo login, string username)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");

			dbUser.Username = username;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException exc)
			{
				var inner = exc.InnerException as NpgsqlException;
				if (inner?.Message?.Contains("IX_Users_Username") ?? false)
					throw new UsernameConflictException();
			}

			return dbUser;
		}

		public Task<Tag[]> SearchTags(string q)
		{
			return _context.Tags
				.Where(t => t.Name.Contains(q))
				.ToArrayAsync();
		}

		public Task<Tag[]> LoadTopTags(int max)
		{
			return _context.Tags
				.Where(t => t.PostsCount > 0)
				.OrderByDescending(t => t.PostsCount)
				.ThenBy(t => t.Name)
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
				await _context.Database.ExecuteSqlCommandAsync(
					"UPDATE \"Posts\" SET \"Votes\" = \"Votes\" + @difference WHERE \"Id\" = @postId",
					new NpgsqlParameter("@difference", difference),
					new NpgsqlParameter("@postId", postId)
				);

			await _context.SaveChangesAsync();

			return difference;
		}

		public Task<UserPostVote[]> GetPostVotingUsers(Guid postId)
		{
			return _context.UserPostVotes
				.AsNoTracking()
				.Include(upv => upv.User)
				.Where(upv => upv.PostId == postId)
				.ToArrayAsync();
		}

		public async Task<UserPostVote[]> GetUserVotedPosts(UserLoginInfo login)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			return await _context.UserPostVotes
				.AsNoTracking()
				.Include(upv => upv.Post)
				.Where(upv => upv.User == dbUser)
				.Select(upv => new UserPostVote
				{
					Votes = upv.Votes,
					PostId = upv.PostId,
					UserId = upv.UserId,
					Post = new Post
					{
						Id = upv.Post.Id,
						Title = upv.Post.Title,
						Votes = upv.Post.Votes
					}
				})
				.ToArrayAsync();
		}
	}
}



