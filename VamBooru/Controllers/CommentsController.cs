using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/posts/{postId}/comments")]
	public class PostCommentsController : Controller
	{
		private readonly IPostCommentsRepository _commentsRepository;

		public PostCommentsController(IPostCommentsRepository commentsRepository)
		{
			_commentsRepository = commentsRepository ?? throw new ArgumentNullException(nameof(commentsRepository));
		}

		[HttpGet("")]
		public async Task<PostCommentViewModel[]> GetAsync([FromRoute] Guid postId)
		{
			var comments = await _commentsRepository.LoadPostCommentsAsync(postId);
			return comments.Select(PostCommentViewModel.From).ToArray();
		}

		[HttpPost("")]
		public async Task<IActionResult> SendAsync([FromRoute] Guid postId, [FromBody] PostCommentViewModel comment)
		{
			if (comment == null) throw new ArgumentNullException(nameof(comment));

			await _commentsRepository.CreatePostCommentAsync(this.GetUserLoginInfo(), postId, comment.Text, DateTimeOffset.UtcNow);

			return StatusCode(201);
		}
	}
}
