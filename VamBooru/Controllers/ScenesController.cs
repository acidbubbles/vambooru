using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Storage;

namespace VamBooru.Controllers
{
	[Route("api/scenes")]
	public class ScenesController : Controller
	{
		private readonly IStorage _storage;

		public ScenesController(IStorage storage)
		{
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("{sceneId}/thumbnail", Name = nameof(GetSceneThumbnailAsync))]
		public async Task<IActionResult> GetSceneThumbnailAsync([FromRoute] Guid sceneId)
		{
			var sceneFile = await _storage.LoadSceneThumbStreamAsync(sceneId);
			if (sceneFile == null) return NotFound();
			return File(sceneFile, "image/jpeg");
		}
	}
}
