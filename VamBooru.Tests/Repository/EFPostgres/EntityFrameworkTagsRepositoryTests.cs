using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkTagsRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkTagsRepository>
	{
		private EntityFrameworkPostsRepository _posts;

		[SetUp]
		public async Task BeforeEach()
		{
			await Initialize();
			await CreateUser();
		}

		[TearDown]
		public void AfterEach()
		{
			Cleanup();
		}

		protected override EntityFrameworkTagsRepository Create(VamBooruDbContext context)
		{
			_posts = new EntityFrameworkPostsRepository(context, new EntityFrameworkUsersRepository(context));
			return new EntityFrameworkTagsRepository(context);
		}

		[Test]
		public async Task TagPostsCount()
		{
			var post1 = await _posts.CreatePostAsync(LoginInfo, "Post1", new[] { "tag1", "tag2" }, new Scene[0], new PostFile[0], "", DateTimeOffset.MinValue);
			var post1ViewModel = PostViewModel.From(post1, false);
			var post2 = await _posts.CreatePostAsync(LoginInfo, "Post2", new[] { "tag2", "tag3" }, new Scene[0], new PostFile[0], "", DateTimeOffset.MinValue);
			var post2ViewModel = PostViewModel.From(post2, false);
			await _posts.CreatePostAsync(LoginInfo, "Post3", new[] { "tag4" }, new Scene[0], new PostFile[0], "", DateTimeOffset.MinValue);

			// Zero by default
			{
				CreateDbContext();
				var tags = await Repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 0},
					new TagViewModel {Name = "tag2", PostsCount = 0},
					new TagViewModel {Name = "tag3", PostsCount = 0},
					new TagViewModel {Name = "tag4", PostsCount = 0}
				}, c => c.MembersToIgnore.Add("*Id"));
			}

			// Increase when published
			{
				post1ViewModel.Published = true;
				await _posts.UpdatePostAsync(LoginInfo, post1ViewModel, DateTimeOffset.UtcNow);
				post2ViewModel.Published = true;
				await _posts.UpdatePostAsync(LoginInfo, post2ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await Repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 1},
					new TagViewModel {Name = "tag2", PostsCount = 2},
					new TagViewModel {Name = "tag3", PostsCount = 1},
					new TagViewModel {Name = "tag4", PostsCount = 0}
				}, c => c.MembersToIgnore.Add("*Id"));
			}

			// Sort by usage
			{
				var tags = await Repository.LoadTopTags(4);
				tags.Select(TagViewModel.From).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag2", PostsCount = 2},
					new TagViewModel {Name = "tag1", PostsCount = 1},
					new TagViewModel {Name = "tag3", PostsCount = 1}
				}, c => c.MembersToIgnore.Add("*Id"));
			}

			// Track changes
			{
				post1ViewModel.Tags = new[] { new TagViewModel { Name = "tag2" }, new TagViewModel { Name = "tag3" } };
				await _posts.UpdatePostAsync(LoginInfo, post1ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await Repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 0},
					new TagViewModel {Name = "tag2", PostsCount = 2},
					new TagViewModel {Name = "tag3", PostsCount = 2},
					new TagViewModel {Name = "tag4", PostsCount = 0},
				}, c => c.MembersToIgnore.Add("*Id"));
			}

			// Decrease when unpublished
			{
				post1ViewModel.Published = false;
				await _posts.UpdatePostAsync(LoginInfo, post1ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await Repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 0},
					new TagViewModel {Name = "tag2", PostsCount = 1},
					new TagViewModel {Name = "tag3", PostsCount = 1},
					new TagViewModel {Name = "tag4", PostsCount = 0},
				}, c => c.MembersToIgnore.Add("*Id"));
			}
		}
	}
}
