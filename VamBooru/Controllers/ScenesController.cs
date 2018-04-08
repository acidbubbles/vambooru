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
		public async Task<Scene[]> BrowseAsync()
		{
			var scenes = await _repository.BrowseScenesAsync();
			return scenes.Select(PrepareForDisplay).ToArray();
		}

		[HttpGet("{id}")]
		public async Task<Scene> GetSceneAsync(Guid id)
		{
			return PrepareForDisplay(await _repository.LoadSceneAsync(id));
		}

		[HttpGet("{id}/files/thumb", Name = nameof(GetThumbnailAsync))]
		public async Task<IActionResult> GetThumbnailAsync(Guid id)
		{
			return File(await _storage.LoadSceneThumbAsync(id), "image/jpeg");
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> SaveSceneAsync([FromBody] Scene scene)
		{
			await _repository.UpdateSceneAsync(scene);
			return NoContent();
		}

		private Scene PrepareForDisplay(Scene scene)
		{
			if (scene.ImageUrl == null)
				scene.ImageUrl = Url.RouteUrl(nameof(GetThumbnailAsync), new {id = scene.Id});
			return scene;
		}
	}
}
