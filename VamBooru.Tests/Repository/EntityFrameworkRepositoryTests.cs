using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository;

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

			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"UserLogins\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Users\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"PostTags\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Tags\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Posts\"");

			var login = await _repository.CreateUserFromLoginAsync("MyScheme", "john.1234", "John Doe", new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero));
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
				}, new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero));

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
				Author = new UserViewModel{Username = _user.Username},
				Title = "New Title",
				Text = "Markdown\nText",
				Published = true,
				Tags = new[] { new TagViewModel { Name = "tag2" }, new TagViewModel { Name = "tag3" } }
			}, new DateTimeOffset(2006, 02, 03, 04, 05, 06, TimeSpan.Zero));

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
					new PostTag {Tag = new Tag {Name = "tag2"}},
					new PostTag {Tag = new Tag {Name = "tag3"}}
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

		private void CreateDbContext()
		{
			_context?.Dispose();
			_context = new TestsDbContextFactory().CreateDbContext(new string[0]);
			_repository = new EntityFrameworkRepository(_context);
		}
	}
}