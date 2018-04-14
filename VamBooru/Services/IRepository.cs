using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IRepository
	{
		Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes);
		Task<Post> LoadPostAsync(Guid id);
		Task<Post[]> BrowsePostsAsync(int page, int pageSize);
		Task UpdatePostAsync(UserLoginInfo login, PostViewModel post);

		Task<UserLogin> CreateUserFromLoginAsync(string scheme, string id, string name);
		Task<User> LoadPrivateUserAsync(UserLoginInfo login);
		Task<User> LoadPrivateUserAsync(string scheme, string id);
		Task<User> LoadPublicUserAsync(string userId);
		Task<User> UpdateUserAsync(UserLoginInfo login, UserViewModel user);

		Task<Tag[]> SearchTags(string q);
	}
}
