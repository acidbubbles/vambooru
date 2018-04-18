using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Repository
{
	public interface IRepository
	{
		Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes, DateTimeOffset now);
		Task<Post> LoadPostAsync(Guid id);
		Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostedSince since, int page, int pageSize);
		Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now);

		Task<SceneFile[]> LoadPostFilesAsync(Guid postId, bool includeBytes);

		Task<UserLogin> CreateUserFromLoginAsync(string scheme, string id, string name, DateTimeOffset now);
		Task<User> LoadPrivateUserAsync(UserLoginInfo login);
		Task<User> LoadPrivateUserAsync(string scheme, string id);
		Task<User> LoadPublicUserAsync(Guid userId);
		Task<User> UpdateUserAsync(UserLoginInfo login, UserViewModel user);

		Task<Tag[]> SearchTags(string q);

		Task<UserPostVote> GetVoteAsync(UserLoginInfo login, Guid postId);
		Task<int> VoteAsync(UserLoginInfo login, Guid postId, int votes);
	}
}
