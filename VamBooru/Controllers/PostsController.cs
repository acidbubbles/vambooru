using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.ViewModels;

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
		public async Task<PostViewModel[]> BrowseAsync([FromQuery] string sort = null, [FromQuery] string direction = null, [FromQuery] string since = null, [FromQuery] int page = 0, [FromQuery] int pageSize = 0)
		{
			var sortParsed = sort != null ? Enum.Parse<PostSortBy>(sort, true) : PostSortBy.Created;
			var sortDirectionParsed = direction != null ? Enum.Parse<PostSortDirection>(direction, true) : PostSortDirection.Down;
			var sinceParsed = since != null ? Enum.Parse<PostedSince>(since, true) : PostedSince.Forever;
			if (page < 0) page = 0;
			if (pageSize <= 0) pageSize = 16;

			if (!AllowsCaching(page, pageSize))
				return await BrowseInternalAsync(page, pageSize, sortParsed, sortDirectionParsed, sinceParsed);

			var key = $"posts:browse:({sortParsed};{sortDirectionParsed};{sinceParsed};{page};{pageSize})";
			var expirationMinutes = sortParsed == PostSortBy.Votes ? 10 : .5; // If you want to see the new stuff, you'll expect it to come fast
			return await _cache.GetOrCreateAsync(key, entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes);
				return BrowseInternalAsync(page, pageSize, sortParsed, sortDirectionParsed, sinceParsed);
			});
		}

		private static bool AllowsCaching(int page, int pageSize)
		{
			return page == 0 && pageSize < 16;
		}

		private async Task<PostViewModel[]> BrowseInternalAsync(int page, int pageSize, PostSortBy sortBy, PostSortDirection sortDirection, PostedSince postedSince)
		{
			var posts = await _repository.BrowsePostsAsync(
				sortBy,
				sortDirection,
				postedSince,
				page,
				pageSize, DateTimeOffset.UtcNow
			);
			return posts.Select(post => PrepareForDisplay(post, true)).ToArray();
		}

		[HttpGet("{postId}")]
		public async Task<PostViewModel> GetPostAsync([FromRoute] Guid postId)
		{
			var viewModel = PrepareForDisplay(await _repository.LoadPostAsync(postId), false);
			viewModel.Files = (await _repository.LoadPostFilesAsync(postId, false)).Select(FileViewModel.From).ToArray();
			return viewModel;
		}

		[HttpPut("{postId}")]
		public async Task<IActionResult> SavePostAsync([FromRoute] Guid postId, [FromBody] PostViewModel post)
		{
			if (post.Id != postId.ToString()) return BadRequest("Mismatch between route post ID and body post ID");
			await _repository.UpdatePostAsync(this.GetUserLoginInfo(), post, DateTimeOffset.UtcNow);
			return NoContent();
		}

		private PostViewModel PrepareForDisplay(Post post, bool optimize)
		{
			var viewModel = PostViewModel.From(post, optimize);

			if (!optimize)
			{
				viewModel.DownloadUrl = Url.RouteUrl(nameof(DownloadController.DownloadPostAsync), new {postId = post.Id});
				foreach (var scene in viewModel.Scenes)
					scene.ImageUrl = Url.RouteUrl(nameof(ScenesController.GetSceneThumbnailAsync), new {sceneId = scene.Id});
				if (viewModel.ImageUrl == null)
					viewModel.ImageUrl = viewModel.Scenes.FirstOrDefault()?.ImageUrl;
			}
			else
			{
				if (viewModel.ImageUrl == null)
				{
					//TODO: Let the user select which image to use as the "default" scene
					var sceneId = post.Scenes.FirstOrDefault()?.Id;
					if (sceneId != null)
						viewModel.ImageUrl = Url.RouteUrl(nameof(ScenesController.GetSceneThumbnailAsync), new {sceneId});
				}
			}

			return viewModel;
		}
	}
}
