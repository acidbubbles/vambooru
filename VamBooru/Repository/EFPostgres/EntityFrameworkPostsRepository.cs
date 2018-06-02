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
	public class EntityFrameworkPostsRepository : EntityFrameworkRepositoryBase, IPostsRepository
	{
		private readonly IUsersRepository _users;

		public EntityFrameworkPostsRepository(VamBooruDbContext dbContext, IUsersRepository users)
			: base(dbContext)
		{
			_users = users ?? throw new ArgumentNullException(nameof(users));
		}

		public Task<Post> CreatePostAsync(UserLoginInfo login, Post post, DateTimeOffset now)
		{
			if (login == null) throw new ArgumentNullException(nameof(login));
			if (post == null) throw new ArgumentNullException(nameof(post));

			return CreatePostAsync(login, post.Title, post.Tags.Select(tag => tag.Tag.Name).ToArray(), post.Scenes.ToArray(), post.PostFiles.ToArray(), post.ThumbnailUrn, now);
		}

		public async Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes, PostFile[] files, string thumbnailUrn, DateTimeOffset now)
		{
			var user = await _users.LoadPrivateUserAsync(login);

			if (user == null) throw new UnauthorizedAccessException("No user found for this login");

			var post = new Post
			{
				Title = title,
				Text = "",
				Author = user,
				ThumbnailUrn = thumbnailUrn,
				DateCreated = now,
				Scenes = new List<Scene>(),
				PostFiles = new List<PostFile>()
			};

			await AssignTagsAsync(post, tags);
			AssignScenesToPost(post, scenes);
			AssignFilesToPost(post, files);

			DbContext.Posts.Add(post);
			await DbContext.SaveChangesAsync();
			return post;
		}

		private void AssignFilesToPost(Post post, IEnumerable<PostFile> files)
		{
			foreach (var file in files)
			{
				if (file.Id != 0) throw new UnauthorizedAccessException("Cannot assign an existing file to a new post");
				DbContext.PostFiles.Add(file);
				post.PostFiles.Add(file);
			}
		}

		private void AssignScenesToPost(Post post, IEnumerable<Scene> scenes)
		{
			foreach (var scene in scenes)
			{
				if (scene.Id != Guid.Empty) throw new UnauthorizedAccessException("Cannot assign an existing scene to a new post");
				DbContext.Scenes.Add(scene);
				post.Scenes.Add(scene);
			}
		}

		public Task<Post> LoadPostAsync(Guid id)
		{
			var query = DbContext.Posts
				.Include(p => p.Author)
				.Include(p => p.Tags)
				.ThenInclude(t => t.Tag)
				.Include(p => p.Scenes)
				.Include(p => p.PostFiles);

			return query.FirstOrDefaultAsync(p => p.Id == id);
		}

		public Task SavePostAsync(Post post)
		{
			return DbContext.SaveChangesAsync();
		}

		public Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page,
			int pageSize, string[] tags, string author, string text, DateTimeOffset now)
		{
			if (page < 0) throw new ArgumentException("Page must be greater than or equal to 0");
			if (pageSize < 1) throw new ArgumentException("Page must be greater than or equal to 1");

			var query = DbContext.Posts
				.AsNoTracking()
				.Include(p => p.Author)
				.Where(p => p.Published);

			if (since != PostedSince.Forever)
			{
				var dateTimeOffset = now.AddDays(-(int) since);
				query = sortBy == PostSortBy.Updated
					? query.Where(p => p.DatePublished >= dateTimeOffset)
					: query.Where(p => p.DateCreated >= dateTimeOffset);
			}

			if (!string.IsNullOrWhiteSpace(author))
			{
				query = query.Where(p => p.Author.Username == author);
			}

			if (!string.IsNullOrWhiteSpace(text))
			{
				//TODO: Replace with ts_vector full text search when available
				query = query.Where(p => p.Text.Contains(text) || p.Title.Contains(text));
			}

			if (tags != null && tags.Length > 0)
			{
				query = tags.Aggregate(query, (curentQuery, tag) => curentQuery.Where(p => p.Tags.Any(pt => pt.Tag.Name == tag)));
			}

			switch (sortBy)
			{
				case PostSortBy.Votes:
					query = sortDirection == PostSortDirection.Down
						? query.OrderByDescending(p => p.Votes).ThenByDescending(p => p.DateCreated)
						: query.OrderBy(p => p.Votes).ThenBy(p => p.DateCreated);
					break;
				case PostSortBy.Created:
					query = sortDirection == PostSortDirection.Down
						? query.OrderByDescending(p => p.DateCreated)
						: query.OrderBy(p => p.DateCreated);
					break;
				case PostSortBy.Updated:
					query = sortDirection == PostSortDirection.Down
						? query.OrderByDescending(p => p.DatePublished)
						: query.OrderBy(p => p.DatePublished);
					break;
				default:
					throw new ArgumentException($"Unknown sort by: {sortBy}", nameof(sortBy));
			}

			query = query
				.Select(p => new Post
				{
					Id = p.Id,
					Title = p.Title,
					Author = p.Author,
					Published = p.Published,
					DateCreated = p.DateCreated,
					DatePublished = p.DatePublished,
					ThumbnailUrn = p.ThumbnailUrn,
					Tags = p.Tags.Select(t => new PostTag {Tag = t.Tag}).ToHashSet(),
					Votes = p.Votes
				})
				.Skip(page * pageSize)
				.Take(pageSize)
				.Include(p => p.Tags)
				.ThenInclude(p => p.Tag);

			return query.ToArrayAsync();
		}

		public async Task<Post[]> BrowseMyPostsAsync(UserLoginInfo login)
		{
			var user = await _users.LoadPrivateUserAsync(login);

			return await DbContext.Posts
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
					Tags = p.Tags.Select(t => new PostTag {Tag = t.Tag}).ToHashSet(),
					Votes = p.Votes,
				})
				.ToArrayAsync();
		}

		public async Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now)
		{
			var user = await _users.LoadPrivateUserAsync(login);
			var dbPost = await LoadPostAsync(Guid.Parse(post.Id));
			var wasPublished = dbPost.Published;
			if (dbPost.Author.Id != user.Id) throw new UnauthorizedAccessException();

			dbPost.Title = post.Title;
			dbPost.Text = post.Text;
			if (!wasPublished && post.Published)
				dbPost.DatePublished = now;
			dbPost.Published = post.Published;
			var tagsAssignationResult = await AssignTagsAsync(dbPost, post.Tags.Select(tag => tag.Name).ToArray());

			await DbContext.SaveChangesAsync();

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

		public async Task UpdatePostScenesAndFiles(Post post, Scene[] newScenes, PostFile[] newFiles)
		{
			DbContext.PostFiles.RemoveRange(post.PostFiles);
			DbContext.Scenes.RemoveRange(post.Scenes);
			AssignScenesToPost(post, newScenes);
			AssignFilesToPost(post, newFiles);
			await DbContext.SaveChangesAsync();
		}

		private async Task ChangeTagsPostsCount(ICollection<Tag> tags, int increment)
		{
			if (tags.Count <= 0)
				return;

			var ids = tags.Select(tag => tag.Id).ToArray();
			await DbContext.Database.ExecuteSqlCommandAsync(
				"UPDATE \"Tags\" SET \"PostsCount\" = \"PostsCount\" + :increment WHERE \"Id\" = ANY(:ids)",
				new NpgsqlParameter("increment", DbType.Int32) {Value = increment},
				// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
				new NpgsqlParameter("ids", NpgsqlDbType.Array | NpgsqlDbType.Uuid) {Value = ids}
			);
		}

		private async Task<TagsAssignationResult> AssignTagsAsync(Post post, string[] tags)
		{
			var result = new TagsAssignationResult();

			if(post.Tags == null) post.Tags = new List<PostTag>();

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
				var postTag = new PostTag {Tag = newTag};
				post.Tags.Add(postTag);
				DbContext.PostTags.Add(postTag);
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
				var dbTag = await DbContext.Tags.SingleOrDefaultAsync(t => t.Name == tag);
				if (dbTag == null)
				{
					dbTag = new Tag {Name = tag};
					DbContext.Tags.Add(dbTag);
				}

				dbTags.Add(dbTag);
			}

			return dbTags.ToArray();
		}
	}
}
