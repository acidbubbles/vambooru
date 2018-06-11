using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public abstract class EntityFrameworkRepositoryTestsBase<T>
	{
		public VamBooruDbContext DbContext;
		public UserLoginInfo LoginInfo;
		public User CurrentUser;
		public T Repository;

		protected async Task Initialize()
		{
			CreateDbContext();

			await EntityFrameworkTestsHelper.ClearAndMarkTestDatabase(DbContext);
		}

		protected async Task CreateUser()
		{
			var login = await new EntityFrameworkUsersRepository(DbContext).LoadOrCreateUserFromLoginAsync("MyScheme", "john.1234", "John Doe", new DateTimeOffset(2001, 02, 03, 04, 05, 06, TimeSpan.Zero));
			LoginInfo = new UserLoginInfo
			{
				Scheme = "MyScheme",
				NameIdentifier = "john.1234"
			};
			CurrentUser = login.Login.User;

			CreateDbContext();
		}

		protected void CreateDbContext()
		{
			DbContext?.Dispose();
			DbContext = new TestsDbContextFactory().CreateDbContext(new string[0]);
			Repository = Create(DbContext);
		}

		protected abstract T Create(VamBooruDbContext context);

		protected void Cleanup()
		{
			DbContext?.Dispose();
			DbContext = null;
		}
	}
}
