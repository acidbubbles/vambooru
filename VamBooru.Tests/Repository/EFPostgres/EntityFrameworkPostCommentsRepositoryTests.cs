using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkPostCommentsRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkPostCommentsRepository>
	{
		private EntityFrameworkUsersRepository _users;
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

		protected override EntityFrameworkPostCommentsRepository Create(VamBooruDbContext context)
		{
			_users = new EntityFrameworkUsersRepository(context);
			_posts = new EntityFrameworkPostsRepository(context, _users);
			return new EntityFrameworkPostCommentsRepository(context, _users);
		}

		[Test]
		public async Task NoCommentsByDefault()
		{
			var post = await _posts.CreatePostAsync(LoginInfo, "Some Title", new string[0], new Scene[0], new PostFile[0], null, DateTimeOffset.MinValue);

			CreateDbContext();
			var comments = await Repository.LoadPostCommentsAsync(post.Id);

			Assert.That(comments, Is.Empty);
		}

		[Test]
		public async Task CreateAndGetComments()
		{
			var post = await _posts.CreatePostAsync(LoginInfo, "Some Title", new string[0], new Scene[0], new PostFile[0], null, DateTimeOffset.MinValue);
			var comment1 = await Repository.CreatePostCommentAsync(LoginInfo, post.Id, "Old Comment", new DateTimeOffset(2001, 02, 03, 01, 00, 00, TimeSpan.Zero));
			var comment2 = await Repository.CreatePostCommentAsync(LoginInfo, post.Id, "New Comment", new DateTimeOffset(2001, 02, 03, 02, 00, 00, TimeSpan.Zero));

			CreateDbContext();
			var comments = await Repository.LoadPostCommentsAsync(post.Id);

			comments.ShouldDeepEqual(new[]
			{
				new PostComment{Author = CurrentUser, Text = "New Comment", DateCreated = comment2.DateCreated},
				new PostComment{Author = CurrentUser, Text = "Old Comment", DateCreated = comment1.DateCreated}
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("PostComment.Post");
				c.MembersToIgnore.Add("User.Logins");
				c.MembersToIgnore.Add("User.PostComments");
			});
		}
	}
}
