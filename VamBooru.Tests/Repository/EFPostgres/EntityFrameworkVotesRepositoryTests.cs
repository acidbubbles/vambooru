using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkVotesRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkVotesRepository>
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

		protected override EntityFrameworkVotesRepository Create(VamBooruDbContext context)
		{
			_users = new EntityFrameworkUsersRepository(context);
			_posts = new EntityFrameworkPostsRepository(context, _users);
			return new EntityFrameworkVotesRepository(context, _users);
		}

		[Test]
		public async Task NoVotesByDefault()
		{
			var post = await _posts.CreatePostAsync(LoginInfo, "Some Title", new string[0], new Scene[0], new PostFile[0], null, DateTimeOffset.MinValue);

			CreateDbContext();
			var updated = await _posts.LoadPostAsync(post.Id);
			var votes = updated.Votes;

			Assert.That(votes, Is.EqualTo(0));
			Assert.That(await Repository.GetPostVotingUsers(post.Id), Is.Empty);
			Assert.That(await Repository.GetUserVotedPosts(LoginInfo), Is.Empty);
		}

		[Test]
		public async Task AddVotesToPost()
		{
			await _users.LoadOrCreateUserFromLoginAsync("Scheme1", "user1", "User 1", DateTimeOffset.UtcNow);
			await _users.LoadOrCreateUserFromLoginAsync("Scheme1", "user2", "User 2", DateTimeOffset.UtcNow);
			await _users.LoadOrCreateUserFromLoginAsync("Scheme1", "user3", "User 3", DateTimeOffset.UtcNow);
			var post = await _posts.CreatePostAsync(LoginInfo, "Some Title", new string[0], new Scene[0], new PostFile[0], null, DateTimeOffset.MinValue);

			CreateDbContext();
			await Repository.VoteAsync(new UserLoginInfo("Scheme1", "user1"), post.Id, 100);
			await Repository.VoteAsync(new UserLoginInfo("Scheme1", "user2"), post.Id, -50);
			await Repository.VoteAsync(new UserLoginInfo("Scheme1", "user3"), post.Id, 25);
			await Repository.VoteAsync(new UserLoginInfo("Scheme1", "user1"), post.Id, 90);

			CreateDbContext();
			var updated = await _posts.LoadPostAsync(post.Id);
			var votes = updated.Votes;

			Assert.That(votes, Is.EqualTo(65));
			Assert.That((await Repository.GetPostVotingUsers(post.Id)).Select(upv => upv.User.Username), Is.EquivalentTo(new[] { "User 1", "User 2", "User 3" }));
			Assert.That((await Repository.GetUserVotedPosts(new UserLoginInfo("Scheme1", "user1"))).Select(upv => upv.PostId), Is.EquivalentTo(new[] { post.Id }));
		}
	}
}
