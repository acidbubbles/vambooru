using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository
{
	public interface IRepository
	{
		Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes, PostFile[] files,
			string thumbnailUrn, DateTimeOffset now);
		Task<Post> LoadPostAsync(Guid id);
		Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page, int pageSize, string[] tags, string author, string text, DateTimeOffset now);
		Task<Post[]> BrowseMyPostsAsync(UserLoginInfo login);
		Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now);

		Task<PostFile[]> LoadPostFilesAsync(Guid postId);
		Task<PostFile> LoadPostFileAsync(Guid postId, string urn);

		Task<LoadOrCreateUserFromLoginResult> LoadOrCreateUserFromLoginAsync(string scheme, string nameIdentifier, string username, DateTimeOffset now);
		Task<User> LoadPrivateUserAsync(UserLoginInfo login);
		Task<User> LoadPrivateUserAsync(string scheme, string id);
		Task<User> LoadPublicUserAsync(string username);
		Task<User> UpdateUserAsync(UserLoginInfo login, string user);

		Task<Tag[]> SearchTags(string q);
		Task<Tag[]> LoadTopTags(int max);

		Task<UserPostVote> GetVoteAsync(UserLoginInfo login, Guid postId);
		Task<int> VoteAsync(UserLoginInfo login, Guid postId, int votes);
	}
}
