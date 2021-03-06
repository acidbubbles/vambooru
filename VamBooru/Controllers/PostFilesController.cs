using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.Storage;

namespace VamBooru.Controllers
{
	[Route("api/scenes")]
	public class PostFilesController : Controller
	{
		private readonly IPostFilesRepository _filesRepository;
		private readonly IStorage _storage;

		public PostFilesController(IPostFilesRepository filesRepository, IStorage storage)
		{
			_filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("posts/{postId}/files", Name = nameof(GetAsync))]
		public async Task<IActionResult> GetAsync([FromRoute] Guid postId, [FromQuery] string urn)
		{
			var file = await _filesRepository.LoadPostFileAsync(postId, urn);
			if (file == null) return NotFound();
			var stream = await _storage.LoadFileStreamAsync(urn, false);
			if (stream == null) return NotFound();
			return File(stream, file.MimeType);
		}
	}
}
