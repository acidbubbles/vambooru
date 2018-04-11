using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/posts")]
	public class PostController : Controller
	{
		private readonly IRepository _repository;

		public PostController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("")]
		public async Task<PostViewModel[]> BrowseAsync([FromQuery] int page = 0, [FromQuery] int pageSize = 10)
		{
			var posts = await _repository.BrowsePostsAsync(page, pageSize);
			return posts.Select(PrepareForDisplay).ToArray();
		}

		[HttpGet("{postId}")]
		public async Task<PostViewModel> GetPostAsync([FromRoute] Guid postId)
		{
			return PrepareForDisplay(await _repository.LoadPostAsync(postId));
		}

		[HttpPut("{postId}")]
		public async Task<IActionResult> SavePostAsync([FromRoute] Guid postId, [FromBody] PostViewModel post)
		{
			if (post.Id != postId.ToString()) return BadRequest("Mismatch between route post ID and body post ID");
			await _repository.UpdatePostAsync(this.GetUserLoginInfo(), post);
			return NoContent();
		}

		private PostViewModel PrepareForDisplay(Post post)
		{
			var viewModel = post.ToViewModel();

			if (viewModel.ImageUrl == null)
			{
				var sceneId = post.Scenes.FirstOrDefault()?.Id;
				if (sceneId != null)
					viewModel.ImageUrl = Url.RouteUrl(nameof(ScenesController.GetSceneThumbnailAsync), new { sceneId });
			}

			return viewModel;
		}
	}
}
