using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Services;

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
			return File(await _storage.LoadSceneThumbAsync(sceneId), "image/jpeg");
		}
	}
}
