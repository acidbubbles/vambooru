using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IRepository
	{
		Task<Post> CreatePostAsync(string title, string[] tags, Scene[] scenes);
		Task<Post> LoadPostAsync(Guid id);
		Task<Post[]> BrowsePostsAsync(int page, int pageSize);
		Task UpdatePostAsync(PostViewModel post);
	}
}
