using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository
{
	public class EntityFrameworkRepositoryTests
	{
		private VamBooruDbContext _context;
		private EntityFrameworkRepository _repository;
		private UserLoginInfo _loginInfo;
		private User _user;

		[SetUp]
		public async Task BeforeEach()
		{
			CreateDbContext();

			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"PostTags\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Tags\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"UserPostVotes\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"SupportFiles\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"SceneFiles\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Scenes\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Posts\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"UserLogins\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Users\"");

			var login = await _repository.LoadOrCreateUserFromLoginAsync("MyScheme", "john.1234", "John Doe", new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero));
			_loginInfo = new UserLoginInfo
			{
				Scheme = "MyScheme",
				NameIdentifier = "john.1234"
			};
			_user = login.User;

			CreateDbContext();
		}

		[TearDown]
		public void AfterEach()
		{
			_context?.Dispose();
		}

		[Test]
		public async Task LoadPrivateUserFromLoginInfo()
		{
			var user = await _repository.LoadPrivateUserAsync(_loginInfo.Scheme, _loginInfo.NameIdentifier);

			user.ShouldDeepEqual(new User
			{
				Logins = new[]
				{
					new UserLogin {Scheme = "MyScheme", NameIdentifier = "john.1234"}
				}.ToList(),
				Username = "John Doe",
				DateSubscribed = new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero)
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
			});
		}

		[Test]
		public async Task LoadUserById()
		{
			var user = await _repository.LoadPublicUserAsync(_user.Id);

			user.ShouldDeepEqual(new User
			{
				// We don't want to load the user logins for public access
				Logins = new List<UserLogin>(),
				Username = "John Doe",
				DateSubscribed = new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero)
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
			});
		}

		[Test]
		public async Task CreateAndGetPosts()
		{
			var saved = await _repository.CreatePostAsync(
				_loginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene",
						Files = new[]
						{
							new SceneFile {Filename = "file.json", Extension = ".json", Bytes = new byte[] {1, 2, 3, 4}}
						}
					}
				}, new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var post = await _repository.LoadPostAsync(saved.Id);

			post.ShouldDeepEqual(new Post
			{
				Title = "My Post",
				Text = "",
				Author = _user,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "my-tag"}}
				}.ToList(),
				Scenes = new[]
				{
					new Scene
					{
						Name = "My Scene",
						// We don't need files
						Files = null
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
				c.MembersToIgnore.Add("User.Scenes");
				c.MembersToIgnore.Add("User.Logins");
			});
		}

		[Test]
		public async Task NewPostsReuseTags()
		{
			var post1 = await _repository.CreatePostAsync(
				_loginInfo,
				"My Post 1",
				new[] {"abc", "def"},
				new Scene[0],
				DateTimeOffset.MinValue
			);

			var post2 = await _repository.CreatePostAsync(
				_loginInfo,
				"My Post 1",
				new[] {"def", "ghi"},
				new Scene[0],
				DateTimeOffset.MinValue
			);

			Assert.That(post1.Tags.Single(t => t.Tag.Name == "def").TagId, Is.EqualTo(post2.Tags.Single(t => t.Tag.Name == "def").TagId), "The Tag should be reused");
		}

		[Test]
		public async Task UpdatePost()
		{
			var saved = await _repository.CreatePostAsync(
				_loginInfo,
				"Old Title",
				new[] {"tag1", "tag2"},
				new Scene[0],
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero));

			CreateDbContext();
			var updated = await _repository.UpdatePostAsync(_loginInfo, new PostViewModel
				{
					Id = saved.Id.ToString(),
					Author = new UserViewModel {Username = _user.Username},
					Title = "New Title",
					Text = "Markdown\nText",
					Published = true,
					Tags = new[] {new TagViewModel {Name = "tag2"}, new TagViewModel {Name = "tag3"}}
				}, new DateTimeOffset(2006, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var post = await _repository.LoadPostAsync(saved.Id);

			post.ShouldDeepEqual(new Post
			{
				Title = "New Title",
				Text = "Markdown\nText",
				Author = _user,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				DatePublished = new DateTimeOffset(2006, 02, 03, 04, 05, 06, TimeSpan.Zero),
				Published = true,
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "tag2", PostsCount = 1}},
					new PostTag {Tag = new Tag {Name = "tag3", PostsCount = 1}}
				}.ToList(),
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("Post.Scenes");
				c.MembersToIgnore.Add("UserLogin.User");
				c.MembersToIgnore.Add("PostTag.PostId");
				c.MembersToIgnore.Add("PostTag.TagId");
				c.MembersToIgnore.Add("PostTag.Post");
				c.MembersToIgnore.Add("Tag.Id");
				c.MembersToIgnore.Add("Scene.Post");
				c.MembersToIgnore.Add("SceneFile.Scene");
				c.MembersToIgnore.Add("User.Scenes");
				c.MembersToIgnore.Add("User.Logins");
			});

			Assert.That(updated.Tags.Single(t => t.Tag.Name == "tag2").TagId, Is.EqualTo(post.Tags.Single(t => t.Tag.Name == "tag2").TagId), "The Tag should be reused");
		}

		[Test]
		public async Task NoVotesByDefault()
		{
			var post = await _repository.CreatePostAsync(_loginInfo, "Some Title", new string[0], new Scene[0], DateTimeOffset.MinValue);

			CreateDbContext();
			var updated = await _repository.LoadPostAsync(post.Id);
			var votes = updated.Votes;

			Assert.That(votes, Is.EqualTo(0));
			Assert.That(await _repository.GetPostVotingUsers(post.Id), Is.Empty);
			Assert.That(await _repository.GetUserVotedPosts(_loginInfo), Is.Empty);
		}

		[Test]
		public async Task AddVotesToPost()
		{
			await _repository.LoadOrCreateUserFromLoginAsync("Scheme1", "user1", "User 1", DateTimeOffset.UtcNow);
			await _repository.LoadOrCreateUserFromLoginAsync("Scheme1", "user2", "User 2", DateTimeOffset.UtcNow);
			await _repository.LoadOrCreateUserFromLoginAsync("Scheme1", "user3", "User 3", DateTimeOffset.UtcNow);
			var post = await _repository.CreatePostAsync(_loginInfo, "Some Title", new string[0], new Scene[0], DateTimeOffset.MinValue);

			CreateDbContext();
			await _repository.VoteAsync(new UserLoginInfo("Scheme1", "user1"), post.Id, 100);
			await _repository.VoteAsync(new UserLoginInfo("Scheme1", "user2"), post.Id, -50);
			await _repository.VoteAsync(new UserLoginInfo("Scheme1", "user3"), post.Id, 25);
			await _repository.VoteAsync(new UserLoginInfo("Scheme1", "user1"), post.Id, 90);

			CreateDbContext();
			var updated = await _repository.LoadPostAsync(post.Id);
			var votes = updated.Votes;

			Assert.That(votes, Is.EqualTo(65));
			Assert.That((await _repository.GetPostVotingUsers(post.Id)).Select(upv => upv.User.Username), Is.EquivalentTo(new[] { "User 1", "User 2", "User 3" }));
			Assert.That((await _repository.GetUserVotedPosts(new UserLoginInfo("Scheme1", "user1"))).Select(upv => upv.PostId), Is.EquivalentTo(new[] { post.Id }));
		}

		[Test]
		public async Task BrowsePostsExcludesNonPublishedPosts()
		{
			await _repository.CreatePostAsync(
				_loginInfo,
				"My Post",
				new[] {"my-tag"},
				new Scene[0],
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var posts = await _repository.BrowsePostsAsync(PostSortBy.Default, PostSortDirection.Default, PostedSince.Default, 0, 1, null, null, DateTimeOffset.MaxValue);

			Assert.That(posts.Length, Is.EqualTo(0));
		}

		[Test]
		public async Task BrowsePostsContainsExpectedFields()
		{
			var saved = await _repository.CreatePostAsync(
				_loginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				}, new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);
			var viewModel = PostViewModel.From(saved, false);
			viewModel.Published = true;
			viewModel.Text = "Some text...";
			await _repository.UpdatePostAsync(_loginInfo, viewModel, new DateTimeOffset(2005, 02, 03, 04, 05, 07, TimeSpan.Zero));

			CreateDbContext();
			var posts = await _repository.BrowsePostsAsync(PostSortBy.Default, PostSortDirection.Default, PostedSince.Default, 0, 1, null, null, DateTimeOffset.MaxValue);

			Assert.That(posts.Length, Is.EqualTo(1));

			posts[0].ShouldDeepEqual(new Post
			{
				Title = "My Post",
				// Text should be excluded
				Text = null,
				Author = _user,
				DateCreated = new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero),
				DatePublished = new DateTimeOffset(2005, 02, 03, 04, 05, 07, TimeSpan.Zero),
				Published = true,
				Tags = new[]
				{
					new PostTag {Tag = new Tag {Name = "my-tag", PostsCount = 1}}
				}.ToList(),
				Scenes = new[]
				{
					new Scene
					{
						Name = "My Scene",
						// We don't need files
						Files = null
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
				c.MembersToIgnore.Add("User.Scenes");
				c.MembersToIgnore.Add("User.Logins");
			});
		}

		[TestCase(PostSortBy.Created, PostSortDirection.Down, PostedSince.Forever, 0, 2, null, null, new[] { "23 hours ago, 30pts", "6 days ago, 90pts" }, TestName = "Sort by created, page 1")]
		[TestCase(PostSortBy.Created, PostSortDirection.Down, PostedSince.Forever, 1, 2, null, null, new[] { "3 weeks ago, 200pts", "11 months ago, 50pts" }, TestName = "Sort by created, page 2")]
		[TestCase(PostSortBy.Updated, PostSortDirection.Up, PostedSince.Forever, 0, 2, null, null, new[] { "2 years ago, 100pts", "11 months ago, 50pts" }, TestName = "Sort by least recently updated, page 1")]
		[TestCase(PostSortBy.Updated, PostSortDirection.Up, PostedSince.Forever, 1, 2, null, null, new[] { "3 weeks ago, 200pts", "6 days ago, 90pts" }, TestName = "Sort by least recently updated, page 2")]
		[TestCase(PostSortBy.Votes, PostSortDirection.Down, PostedSince.Forever, 0, 2, null, null, new[] { "3 weeks ago, 200pts", "2 years ago, 100pts" }, TestName = "Sort by votes, page 1")]
		[TestCase(PostSortBy.Votes, PostSortDirection.Down, PostedSince.Forever, 1, 2, null, null, new[] { "6 days ago, 90pts", "11 months ago, 50pts" }, TestName = "Sort by votes, page 2")]
		[TestCase(PostSortBy.Created, PostSortDirection.Up, PostedSince.LastYear, 0, 1, null, null, new[] { "11 months ago, 50pts" }, TestName = "Since last year")]
		[TestCase(PostSortBy.Created, PostSortDirection.Up, PostedSince.LastMonth, 0, 1, null, null, new[] { "3 weeks ago, 200pts" }, TestName = "Since last month")]
		[TestCase(PostSortBy.Created, PostSortDirection.Up, PostedSince.LastDay, 0, 1, null, null, new[] { "23 hours ago, 30pts" }, TestName = "Since yesterday")]
		[TestCase(PostSortBy.Created, PostSortDirection.Down, PostedSince.Forever, 0, 2, new[] { "tag1" }, null, new[] { "3 weeks ago, 200pts", "2 years ago, 100pts" }, TestName = "Search by one tag")]
		[TestCase(PostSortBy.Created, PostSortDirection.Down, PostedSince.Forever, 0, 2, new[] { "tag1", "tag2" }, null, new[] { "3 weeks ago, 200pts" }, TestName = "Search by two tags")]
		[TestCase(PostSortBy.Created, PostSortDirection.Down, PostedSince.Forever, 0, 2, null, "cool", new[] { "6 days ago, 90pts" }, TestName = "Full text search")]
		public async Task BrowsePostsFilters(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page, int pageSize, string[] tags, string search, string[] expected)
		{
			Scene[] CreateScenes() => new[]
			{
				new Scene
				{
					Name = "My Scene"
				}
			};

			async Task CreatePost(string title, string text, int votes, DateTimeOffset created, params string[] postTags)
			{
				var post = await _repository.CreatePostAsync(
					_loginInfo,
					title,
					postTags,
					CreateScenes(),
					created
				);

				var viewModel = PostViewModel.From(post, false);
				viewModel.Published = true;
				viewModel.Text = text;
				await _repository.UpdatePostAsync(_loginInfo, viewModel, post.DateCreated.AddDays(1));

				await _repository.VoteAsync(_loginInfo, post.Id, votes);
			}

			var now = new DateTimeOffset(2000, 01, 01, 01, 01, 01, TimeSpan.Zero);
			await CreatePost("2 years ago, 100pts", "", 100, now.AddYears(-2), "tag1");
			await CreatePost("11 months ago, 50pts", "", 50, now.AddMonths(-11), "tag2");
			await CreatePost("3 weeks ago, 200pts", "", 200, now.AddDays(-7 * 3), "tag1", "tag2");
			await CreatePost("6 days ago, 90pts", "cool one", 90, now.AddDays(-6));
			await CreatePost("23 hours ago, 30pts", "", 30, now.AddHours(-1));

			CreateDbContext();
			var posts = await _repository.BrowsePostsAsync(sortBy, sortDirection, since, page, pageSize, tags, search, now);

			CollectionAssert.AreEqual(expected, posts.Select(post => post.Title).ToArray());
		}

		[TestCase(true)]
		[TestCase(false)]
		public async Task GetPostFiles(bool includeBytes)
		{
			var saved = await _repository.CreatePostAsync(
				_loginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				}, new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);
			var scene = saved.Scenes.First();
			scene.Files = scene.Files ?? new List<SceneFile>();

			//TODO: Right now we mix Storage and Repository a lot. Bytes should move out eventually
			saved.SupportFiles.Add(new SupportFile { Filename = "sound.wav", Bytes = new byte[] { 1, 2, 3 } });
			scene.Files.Add(new SceneFile {Filename = "My Scene.json", Extension = ".json", Bytes = new byte[] {5, 6}});
			scene.Files.Add(new SceneFile {Filename = "My Scene.jpg", Extension = ".jpg", Bytes = new byte[] {7, 8}});
			await _context.SaveChangesAsync();

			CreateDbContext();
			var files = await _repository.LoadPostFilesAsync(saved.Id, includeBytes);

			files.ShouldDeepEqual(new IFileModel[]
			{
				new SceneFile {Filename = "My Scene.json", Bytes = includeBytes ? new byte[] {5, 6} : null},
				new SceneFile {Filename = "My Scene.jpg", Bytes = includeBytes ? new byte[] {7, 8} : null},
				new SupportFile {Filename = "sound.wav", Bytes = includeBytes ? new byte[] {1, 2, 3} : null}
			}, c =>
			{
				c.IgnoreCollectionOrder = true;
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("SupportFile.Post");
				c.MembersToIgnore.Add("SceneFile.Scene");
			});
		}

		[Test]
		public async Task TagPostsCount()
		{
			var post1 = await _repository.CreatePostAsync(_loginInfo, "Post1", new[] { "tag1", "tag2" }, new Scene[0], DateTimeOffset.MinValue);
			var post1ViewModel = PostViewModel.From(post1, false);
			var post2 = await _repository.CreatePostAsync(_loginInfo, "Post2", new[] { "tag2", "tag3" }, new Scene[0], DateTimeOffset.MinValue);
			var post2ViewModel = PostViewModel.From(post2, false);
			await _repository.CreatePostAsync(_loginInfo, "Post3", new[] { "tag4" }, new Scene[0], DateTimeOffset.MinValue);

			// Zero by default
			{
				CreateDbContext();
				var tags = await _repository.SearchTags("tag");
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
				await _repository.UpdatePostAsync(_loginInfo, post1ViewModel, DateTimeOffset.UtcNow);
				post2ViewModel.Published = true;
				await _repository.UpdatePostAsync(_loginInfo, post2ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await _repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 1},
					new TagViewModel {Name = "tag2", PostsCount = 2},
					new TagViewModel {Name = "tag3", PostsCount = 1},
					new TagViewModel {Name = "tag4", PostsCount = 0}
				}, c => c.MembersToIgnore.Add("*Id"));
			}

			// Track changes
			{
				post1ViewModel.Tags = new[] { new TagViewModel { Name = "tag2" }, new TagViewModel { Name = "tag3" } };
				await _repository.UpdatePostAsync(_loginInfo, post1ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await _repository.SearchTags("tag");
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
				await _repository.UpdatePostAsync(_loginInfo, post1ViewModel, DateTimeOffset.UtcNow);

				CreateDbContext();
				var tags = await _repository.SearchTags("tag");
				tags.Select(TagViewModel.From).OrderBy(t => t.Name).ToArray().ShouldDeepEqual(new[]
				{
					new TagViewModel {Name = "tag1", PostsCount = 0},
					new TagViewModel {Name = "tag2", PostsCount = 1},
					new TagViewModel {Name = "tag3", PostsCount = 1},
					new TagViewModel {Name = "tag4", PostsCount = 0},
				}, c => c.MembersToIgnore.Add("*Id"));
			}
		}

		private void CreateDbContext()
		{
			_context?.Dispose();
			_context = new TestsDbContextFactory().CreateDbContext(new string[0]);
			_repository = new EntityFrameworkRepository(_context);
		}
	}
}



