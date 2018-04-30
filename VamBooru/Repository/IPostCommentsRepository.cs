using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository
{
	public interface IPostCommentsRepository
	{
		Task<PostComment> CreatePostCommentAsync(UserLoginInfo login, Guid postId, string text, DateTimeOffset now);
		Task<PostComment[]> LoadPostCommentsAsync(Guid postId);
	}
}
