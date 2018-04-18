using System;
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

		[SetUp]
		public async Task BeforeEach()
		{
			_context = new TestsDbContextFactory().CreateDbContext(new string[0]);
			_repository = new EntityFrameworkRepository(_context);

			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"UserLogins\"");
			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"Users\"");
		}

		[TearDown]
		public void AfterEach()
		{
			_context?.Dispose();
		}

		[Test]
		public async Task CanCreateAndGetUsersByLogin()
		{
			await _repository.CreateUserFromLoginAsync("MyScheme", "john.1234", "John Doe", new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero));
			var user = await _repository.LoadPrivateUserAsync("MyScheme", "john.1234");

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
	}
}
