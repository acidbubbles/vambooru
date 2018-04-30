using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkUsersRepository : EntityFrameworkRepositoryBase, IUsersRepository
	{
		public EntityFrameworkUsersRepository(VamBooruDbContext dbContext)
			: base(dbContext)
		{
		}

		public async Task<LoadOrCreateUserFromLoginResult> LoadOrCreateUserFromLoginAsync(string scheme,
			string nameIdentifier, string username, DateTimeOffset now)
		{
			var login = await DbContext.UserLogins
				.Include(ul => ul.User)
				.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == nameIdentifier);

			if (login != null)
				return new LoadOrCreateUserFromLoginResult
				{
					Login = login,
					Result = LoadOrCreateUserFromLoginResultTypes.ExistingUser
				};

			var user = new User {Username = username, DateSubscribed = now};
			DbContext.Users.Add(user);

			login = new UserLogin {User = user, Scheme = scheme, NameIdentifier = nameIdentifier};
			DbContext.UserLogins.Add(login);

			try
			{
				await DbContext.SaveChangesAsync();
			}
			catch (DbUpdateException exc)
			{
				var inner = exc.InnerException as NpgsqlException;
				if (inner?.Message?.Contains("IX_Users_Username") ?? false)
					throw new UsernameConflictException();
			}

			return new LoadOrCreateUserFromLoginResult
			{
				Login = login,
				Result = LoadOrCreateUserFromLoginResultTypes.NewUser
			};
		}

		public Task<User> LoadPrivateUserAsync(UserLoginInfo info)
		{
			return LoadPrivateUserAsync(info.Scheme, info.NameIdentifier);
		}

		public async Task<User> LoadPrivateUserAsync(string scheme, string id)
		{
			var login = await DbContext.UserLogins
				.Include(l => l.User)
				.FirstOrDefaultAsync(l => l.Scheme == scheme && l.NameIdentifier == id);

			return login?.User;
		}

		public Task<User> LoadPublicUserAsync(string username)
		{
			return DbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
		}

		public async Task<User> UpdateUserAsync(UserLoginInfo login, string username)
		{
			var dbUser = await LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");

			dbUser.Username = username;

			try
			{
				await DbContext.SaveChangesAsync();
			}
			catch (DbUpdateException exc)
			{
				var inner = exc.InnerException as NpgsqlException;
				if (inner?.Message?.Contains("IX_Users_Username") ?? false)
					throw new UsernameConflictException();
			}

			return dbUser;
		}
	}
}
