using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Services
{
	public class EntityFrameworkRepository : IRepository
	{
		private readonly VamBooruDbContext _context;

		public EntityFrameworkRepository(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes)
		{
			var dbLogin = await _context.UserLogins
				.Include(l => l.User)
				.FirstOrDefaultAsync(l => l.Scheme == login.Scheme && l.NameIdentifier == login.NameIdentifier);

			if(dbLogin?.User == null) throw new UnauthorizedAccessException("No user found for this login");

			var post = new Post
			{
				Title = title,
				Author = dbLogin.User
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
				.Include(s => s.Author)
				.Include(s => s.Tags)
				.ThenInclude(t => t.Tag)
				.Include(s => s.Scenes)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public Task<Post[]> BrowsePostsAsync(int page, int pageSize)
		{
			return _context.Posts
				.AsNoTracking()
				.Include(s => s.Author)
				.Include(s => s.Tags)
				.ThenInclude(t => t.Tag)
				.Include(s => s.Scenes)
				.Where(s => s.Published)
				.Skip(page * pageSize)
				.Take(pageSize)
				.ToArrayAsync();
		}

		public async Task UpdatePostAsync(PostViewModel post)
		{
			//TODO: Validate input ID, owner, etc.
			var dbPost = await LoadPostAsync(Guid.Parse(post.Id));
			dbPost.Title = post.Title;
			if (!dbPost.Published && post.Published) dbPost.DatePublished = DateTimeOffset.UtcNow;
			dbPost.Published = post.Published;
			await AssignTagsAsync(dbPost, post.Tags.Select(tag => tag.Name).ToArray());
			await _context.SaveChangesAsync();
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
				var postTag = new PostTag { Post = post, Tag = newTag };
				_context.PostTags.Add(postTag);
				post.Tags.Add(postTag);
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

		public async Task<UserLogin> CreateUserFromLoginAsync(string scheme, string id, string name)
		{
			var login = await _context.UserLogins.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == name);

			if (login != null) return login;

			var user = new User {Username = name};
			_context.Users.Add(user);

			login = new UserLogin { User = user, Scheme = scheme, NameIdentifier = id };
			_context.UserLogins.Add(login);

			await _context.SaveChangesAsync();
			return login;
		}
	}
}

