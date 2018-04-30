using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkPostsRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkPostsRepository>
	{
		private EntityFrameworkUsersRepository _users;
		private EntityFrameworkVotesRepository _votes;

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

		protected override EntityFrameworkPostsRepository Create(VamBooruDbContext context)
		{
			_users = new EntityFrameworkUsersRepository(context);
			_votes = new EntityFrameworkVotesRepository(context, _users);
			return new EntityFrameworkPostsRepository(context, _users);
		}

		[Test]
		public async Task CreateAndGetPosts()
		{
			var saved = await Repository.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				},
				new[]
				{
					new PostFile {Filename = "file.json", Urn = "urn:vambooru:tests:0001", Compressed = true},
					new PostFile {Filename = "file.jpg", Urn = "urn:vambooru:tests:0002"}
				},
				"urn:vambooru:tests:0002",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var post = await Repository.LoadPostAsync(saved.Id);

			post.ShouldDeepEqual(new Post
			{
				Title = "My Post",
				Text = "",
				Author = CurrentUser,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				ThumbnailUrn = "urn:vambooru:tests:0002",
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "my-tag"}}
				}.ToList(),
				Scenes = new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				}.ToList()
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
				c.MembersToIgnore.Add("PostTag.PostId");
				c.MembersToIgnore.Add("PostTag.TagId");
				c.MembersToIgnore.Add("PostTag.Post");
				c.MembersToIgnore.Add("Tag.Id");
				c.MembersToIgnore.Add("Scene.Post");
				c.MembersToIgnore.Add("SceneFile.Scene");
				c.MembersToIgnore.Add("User.Posts");
				c.MembersToIgnore.Add("User.Logins");
			});
		}

		[Test]
		public async Task NewPostsReuseTags()
		{
			var post1 = await Repository.CreatePostAsync(
				LoginInfo,
				"My Post 1",
				new[] {"abc", "def"},
				new Scene[0],
				new PostFile[0],
				"",
				DateTimeOffset.MinValue
			);

			var post2 = await Repository.CreatePostAsync(
				LoginInfo,
				"My Post 1",
				new[] {"def", "ghi"},
				new Scene[0], new PostFile[0], "",
				DateTimeOffset.MinValue
			);

			Assert.That(post1.Tags.Single(t => t.Tag.Name == "def").TagId, Is.EqualTo(post2.Tags.Single(t => t.Tag.Name == "def").TagId), "The Tag should be reused");
		}

		[Test]
		public async Task UpdatePost()
		{
			var saved = await Repository.CreatePostAsync(
				LoginInfo,
				"Old Title",
				new[] {"tag1", "tag2"},
				new Scene[0],
				new PostFile[0],
				"urn:vambooru:tests:0000",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero));

			CreateDbContext();
			var updated = await Repository.UpdatePostAsync(LoginInfo, new PostViewModel
				{
					Id = saved.Id.ToString(),
					Author = new UserViewModel {Username = CurrentUser.Username},
					Title = "New Title",
					Text = "Markdown\nText",
					Published = true,
					Tags = new[]
					{
						new TagViewModel {Name = "tag2"},
						new TagViewModel {Name = "tag3"}
					}
				}, new DateTimeOffset(2006, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var post = await Repository.LoadPostAsync(saved.Id);
			//TODO: This can be removed if we use a predictable index (long)
			post.Tags = post.Tags.OrderBy(t => t.Tag.Name).ToList();

			post.ShouldDeepEqual(new Post
			{
				Title = "New Title",
				Text = "Markdown\nText",
				Author = CurrentUser,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				DatePublished = new DateTimeOffset(2006, 02, 03, 04, 05, 06, TimeSpan.Zero),
				Published = true,
				ThumbnailUrn = "urn:vambooru:tests:0000",
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "tag2", PostsCount = 1}},
					new PostTag {Tag = new Tag {Name = "tag3", PostsCount = 1}}
				}.ToList()
			}, c =>
			{
				c.MaxDifferences = 3;
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("Post.Scenes");
				c.MembersToIgnore.Add("UserLogin.User");
				c.MembersToIgnore.Add("PostTag.PostId");
				c.MembersToIgnore.Add("PostTag.TagId");
				c.MembersToIgnore.Add("PostTag.Post");
				c.MembersToIgnore.Add("Tag.Id");
				c.MembersToIgnore.Add("Scene.Post");
				c.MembersToIgnore.Add("SceneFile.Scene");
				c.MembersToIgnore.Add("User.Posts");
				c.MembersToIgnore.Add("User.Logins");
			});

			Assert.That(updated.Tags.Single(t => t.Tag.Name == "tag2").TagId, Is.EqualTo(post.Tags.Single(t => t.Tag.Name == "tag2").TagId), "The Tag should be reused");
		}

		[Test]
		public async Task BrowsePostsExcludesNonPublishedPosts()
		{
			await Repository.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new Scene[0],
				new PostFile[0],
				"",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var posts = await Repository.BrowsePostsAsync(PostSortBy.Default, PostSortDirection.Default, PostedSince.Default, 0, 1, null, null, null, DateTimeOffset.MaxValue);

			Assert.That(posts.Length, Is.EqualTo(0));
		}

		[Test]
		public async Task BrowsePostsContainsExpectedFields()
		{
			var saved = await Repository.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				},
				new[]
				{
					new PostFile {Filename = "file.json", Urn = "urn:vambooru:tests:0001", Compressed = true},
					new PostFile {Filename = "file.jpg", Urn = "urn:vambooru:tests:0002"}
				},
				"urn:vambooru:tests:0002",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);
			var viewModel = PostViewModel.From(saved, false);
			viewModel.Published = true;
			viewModel.Text = "Some text...";
			await Repository.UpdatePostAsync(LoginInfo, viewModel, new DateTimeOffset(2005, 02, 03, 04, 05, 07, TimeSpan.Zero));

			CreateDbContext();
			var posts = await Repository.BrowsePostsAsync(PostSortBy.Default, PostSortDirection.Default, PostedSince.Default, 0, 1, null, null, null, DateTimeOffset.MaxValue);

			Assert.That(posts.Length, Is.EqualTo(1));

			posts[0].ShouldDeepEqual(new Post
			{
				Title = "My Post",
				// Text should be excluded
				Text = null,
				Author = CurrentUser,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				DatePublished = new DateTimeOffset(2005, 02, 03, 04, 05, 07, TimeSpan.Zero),
				Published = true,
				ThumbnailUrn = "urn:vambooru:tests:0002",
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "my-tag", PostsCount = 1}}
				}.ToList(),
				Scenes = new List<Scene>()
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
				c.MembersToIgnore.Add("PostTag.PostId");
				c.MembersToIgnore.Add("PostTag.TagId");
				c.MembersToIgnore.Add("PostTag.Post");
				c.MembersToIgnore.Add("Tag.Id");
				c.MembersToIgnore.Add("Scene.Post");
				c.MembersToIgnore.Add("SceneFile.Scene");
				c.MembersToIgnore.Add("User.Posts");
				c.MembersToIgnore.Add("User.Logins");
			});
		}

		public class BrowsePostFilterTestCase
		{
			public PostSortBy SortBy { get; set; }
			public PostSortDirection SortDirection { get; set; }
			public PostedSince Since { get; set; }
			public int Page { get; set; }
			public int PageSize { get; set; }
			public string[] Tags { get; set; }
			public string Author { get; set; }
			public string Text { get; set; }
			public string[] Expected { get; set; }
			public string TestName { get;set; }

			public override string ToString()
			{
				return TestName;
			}
		}

		public static IEnumerable<BrowsePostFilterTestCase> BrowsePostFilterTestCases()
		{
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"23 hours ago, 30pts", "6 days ago, 90pts"},
				TestName = "Sort by created, page 1"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 1,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts", "11 months ago, 50pts"},
				TestName = "Sort by created, page 2"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Updated,
				SortDirection = PostSortDirection.Up,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"2 years ago, 100pts", "11 months ago, 50pts"},
				TestName = "Sort by least recently updated, page 1"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Updated,
				SortDirection = PostSortDirection.Up,
				Since = PostedSince.Forever,
				Page = 1,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts", "6 days ago, 90pts"},
				TestName = "Sort by least recently updated, page 2"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Votes,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts", "2 years ago, 100pts"},
				TestName = "Sort by votes, page 1"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Votes,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 1,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"6 days ago, 90pts", "11 months ago, 50pts"},
				TestName = "Sort by votes, page 2"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Up,
				Since = PostedSince.LastYear,
				Page = 0,
				PageSize = 1,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"11 months ago, 50pts"},
				TestName = "Since last year"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Up,
				Since = PostedSince.LastMonth,
				Page = 0,
				PageSize = 1,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts"},
				TestName = "Since last month"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Up,
				Since = PostedSince.LastDay,
				Page = 0,
				PageSize = 1,
				Tags = null,
				Author = null,
				Text = null,
				Expected = new[] {"23 hours ago, 30pts"},
				TestName = "Since yesterday"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = new[] {"tag1"},
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts", "2 years ago, 100pts"},
				TestName = "Search by one tag"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = new[] {"tag1", "tag2"},
				Author = null,
				Text = null,
				Expected = new[] {"3 weeks ago, 200pts"},
				TestName = "Search by two tags"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = "cool",
				Expected = new[] {"6 days ago, 90pts"},
				TestName = "Text search in post text"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 2,
				Tags = null,
				Author = null,
				Text = "weeks",
				Expected = new[] {"3 weeks ago, 200pts"},
				TestName = "Text search in post title"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 1,
				Tags = null,
				Author = "John Doe",
				Text = null,
				Expected = new[] {"23 hours ago, 30pts"},
				TestName = "Filter by current user"
			};
			yield return new BrowsePostFilterTestCase
			{
				SortBy = PostSortBy.Created,
				SortDirection = PostSortDirection.Down,
				Since = PostedSince.Forever,
				Page = 0,
				PageSize = 1,
				Tags = null,
				Author = "Abbie Bibbo",
				Text = null,
				Expected = new string[0],
				TestName = "Filter by non-existent user"
			};
		}

		[Test]
		[TestCaseSource(nameof(BrowsePostFilterTestCases))]
		public async Task BrowsePostsFilters(BrowsePostFilterTestCase test)
		{
			Scene[] CreateScenes() => new[]
			{
				new Scene
				{
					Name = "My Scene"
				}
			};

			async Task CreatePost(string postTitle, string postText, int votes, DateTimeOffset created, params string[] postTags)
			{
				var post = await Repository.CreatePostAsync(
					LoginInfo,
					postTitle,
					postTags,
					CreateScenes(),
					new PostFile[0],
					"",
					created
				);

				var viewModel = PostViewModel.From(post, false);
				viewModel.Published = true;
				viewModel.Text = postText;
				await Repository.UpdatePostAsync(LoginInfo, viewModel, post.DateCreated.AddDays(1));

				await _votes.VoteAsync(LoginInfo, post.Id, votes);
			}

			var now = new DateTimeOffset(2000, 01, 01, 01, 01, 01, TimeSpan.Zero);
			await CreatePost("2 years ago, 100pts", "", 100, now.AddYears(-2), "tag1");
			await CreatePost("11 months ago, 50pts", "", 50, now.AddMonths(-11), "tag2");
			await CreatePost("3 weeks ago, 200pts", "", 200, now.AddDays(-7 * 3), "tag1", "tag2");
			await CreatePost("6 days ago, 90pts", "cool one", 90, now.AddDays(-6));
			await CreatePost("23 hours ago, 30pts", "", 30, now.AddHours(-1));

			CreateDbContext();
			var posts = await Repository.BrowsePostsAsync(test.SortBy, test.SortDirection, test.Since, test.Page, test.PageSize, test.Tags, test.Author, test.Text, now);

			CollectionAssert.AreEqual(test.Expected, posts.Select(post => post.Title).ToArray());
		}

		[Test]
		public async Task MyPostsIncludesNonPublishedPosts()
		{
			await Repository.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new Scene[0],
				new PostFile[0],
				"",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var posts = await Repository.BrowseMyPostsAsync(LoginInfo);

			CollectionAssert.AreEqual(new[] { "My Post" }, posts.Select(post => post.Title).ToArray());
		}
	}
}
