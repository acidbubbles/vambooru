using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/posts")]
	public class PostsController : Controller
	{
		private readonly IRepository _repository;
		private readonly IMemoryCache _cache;

		public PostsController(IRepository repository, IMemoryCache cache)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		[HttpGet("")]
		public async Task<PostViewModel[]> BrowseAsync([FromQuery] string sort = null, [FromQuery] string since = null, [FromQuery] int page = 0, [FromQuery] int pageSize = 16)
		{
			var sortParsed = sort != null ? Enum.Parse<PostSortBy>(sort, true) : PostSortBy.Default;
			var sinceParsed = since != null ? Enum.Parse<PostedSince>(since, true) : PostedSince.Default;

			if (!AllowsCaching(page, pageSize))
				return await BrowseInternalAsync(page, pageSize, sortParsed, sinceParsed);

			var key = $"posts:browse:({sortParsed};{sinceParsed};{page};{pageSize})";
			var expirationMinutes = sortParsed == PostSortBy.Newest ? 1 : 10;
			return await _cache.GetOrCreateAsync(key, entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes);
				return BrowseInternalAsync(page, pageSize, sortParsed, sinceParsed);
			});
		}

		private static bool AllowsCaching(int page, int pageSize)
		{
			return page == 0 && pageSize < 16;
		}

		private async Task<PostViewModel[]> BrowseInternalAsync(int page, int pageSize, PostSortBy sortBy, PostedSince postedSince)
		{
			var posts = await _repository.BrowsePostsAsync(
				sortBy,
				postedSince,
				page >= 0 ? page : 0,
				pageSize > 0 ? pageSize : 0
			);
			return posts.Select(post => PrepareForDisplay(post, true)).ToArray();
		}

		[HttpGet("{postId}")]
		public async Task<PostViewModel> GetPostAsync([FromRoute] Guid postId)
		{
			return PrepareForDisplay(await _repository.LoadPostAsync(postId), false);
		}

		[HttpPut("{postId}")]
		public async Task<IActionResult> SavePostAsync([FromRoute] Guid postId, [FromBody] PostViewModel post)
		{
			if (post.Id != postId.ToString()) return BadRequest("Mismatch between route post ID and body post ID");
			await _repository.UpdatePostAsync(this.GetUserLoginInfo(), post);
			return NoContent();
		}

		private PostViewModel PrepareForDisplay(Post post, bool optimize)
		{
			var viewModel = post.ToViewModel(optimize);

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
