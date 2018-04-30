using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Repository.EFPostgres;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkUsersRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkUsersRepository>
	{
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

		protected override EntityFrameworkUsersRepository Create(VamBooruDbContext context) => new EntityFrameworkUsersRepository(context);

		[Test]
		public async Task LoadPrivateUserFromLoginInfo()
		{
			var user = await Repository.LoadPrivateUserAsync(LoginInfo.Scheme, LoginInfo.NameIdentifier);

			user.ShouldDeepEqual(new User
			{
				Logins = new[]
				{
					new UserLogin {Scheme = "MyScheme", NameIdentifier = "john.1234"}
				}.ToHashSet(),
				Username = "John Doe",
				Role = UserRoles.Standard,
				DateSubscribed = new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero)
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
			});
		}

		[Test]
		public void UsernamesAreUnique()
		{
			Assert.ThrowsAsync<UsernameConflictException>(() => Repository.LoadOrCreateUserFromLoginAsync("MyScheme", "some.other.john", "John Doe", DateTimeOffset.UtcNow));
		}

		[Test]
		public async Task UpdateUser()
		{
			var user = await Repository.UpdateUserAsync(
				LoginInfo,
				"Happy Panda"
			);

			user.ShouldDeepEqual(new User
			{
				// We don't want to load the user logins for public access
				Logins = new[]
				{
					new UserLogin {Scheme = LoginInfo.Scheme, NameIdentifier = LoginInfo.NameIdentifier}
				}.ToHashSet(),
				Username = "Happy Panda",
				Role = UserRoles.Standard,
				DateSubscribed = new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero)
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
			});
		}

		[Test]
		public async Task LoadUserByUsername()
		{
			var user = await Repository.LoadPublicUserAsync(CurrentUser.Username);

			user.ShouldDeepEqual(new User
			{
				Username = "John Doe",
				Role = UserRoles.Standard,
				DateSubscribed = new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero)
			}, c =>
			{
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("UserLogin.User");
			});
		}
	}
}
