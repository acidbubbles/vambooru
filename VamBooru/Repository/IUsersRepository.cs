using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository
{
	public interface IUsersRepository
	{
		Task<LoadOrCreateUserFromLoginResult> LoadOrCreateUserFromLoginAsync(string scheme, string nameIdentifier, string username, DateTimeOffset now);
		Task<User> LoadPrivateUserAsync(UserLoginInfo login);
		Task<User> LoadPrivateUserAsync(string scheme, string id);
		Task<User> LoadPublicUserAsync(string username);
		Task<User> UpdateUserAsync(UserLoginInfo login, string user);
	}
}