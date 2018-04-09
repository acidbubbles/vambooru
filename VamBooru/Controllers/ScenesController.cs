using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/scenes")]
	public class ScenesController : Controller
	{
		private readonly IRepository _repository;
		private readonly IStorage _storage;

		public ScenesController(IRepository repository, IStorage storage)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("")]
		public async Task<SceneViewModel[]> BrowseAsync([FromQuery] int page = 0, [FromQuery] int pageSize = 10)
		{
			var scenes = await _repository.BrowseScenesAsync(page, pageSize);
			return scenes.Select(PrepareForDisplay).ToArray();
		}

		[HttpGet("{id}")]
		public async Task<SceneViewModel> GetSceneAsync(Guid id)
		{
			return PrepareForDisplay(await _repository.LoadSceneAsync(id));
		}

		[HttpGet("{id}/files/thumb", Name = nameof(GetThumbnailAsync))]
		public async Task<IActionResult> GetThumbnailAsync(Guid id)
		{
			return File(await _storage.LoadSceneThumbAsync(id), "image/jpeg");
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> SaveSceneAsync([FromBody] SceneViewModel scene)
		{
			await _repository.UpdateSceneAsync(scene);
			return NoContent();
		}

		private SceneViewModel PrepareForDisplay(Scene scene)
		{
			var viewModel = scene.ToViewModel();
			if (viewModel.ImageUrl == null)
				viewModel.ImageUrl = Url.RouteUrl(nameof(GetThumbnailAsync), new {id = scene.Id});
			return viewModel;
		}
	}
}
