using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository
{
	public interface IPostsRepository
	{
		Task<Post> CreatePostAsync(UserLoginInfo login, Post post, DateTimeOffset now);
		Task<Post> CreatePostAsync(UserLoginInfo login, string title, string[] tags, Scene[] scenes, PostFile[] files, string thumbnailUrn, DateTimeOffset now);
		Task<Post> LoadPostAsync(Guid id);
		Task SavePostAsync(Post post);
		Task<Post[]> BrowsePostsAsync(PostSortBy sortBy, PostSortDirection sortDirection, PostedSince since, int page, int pageSize, string[] tags, string author, string text, DateTimeOffset now);
		Task<Post[]> BrowseMyPostsAsync(UserLoginInfo login);
		Task<Post> UpdatePostAsync(UserLoginInfo login, PostViewModel post, DateTimeOffset now);
		Task UpdatePostScenesAndFiles(Post post, Scene[] newScenes, PostFile[] newFiles);
	}
}
