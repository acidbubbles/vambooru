using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/[controller]")]
	public class ScenesController : Controller
	{
		private readonly IRepository _repository;
		private readonly IStorage _storage;
		private readonly IProjectParser _projectParser;

		public ScenesController(IRepository repository, IStorage storage, IProjectParser projectParser)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			_projectParser = projectParser ?? throw new ArgumentNullException(nameof(projectParser));
		}

		[HttpGet("[action]")]
		public Task<Scene[]> Browse()
		{
			return _repository.BrowseScenesAsync();
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Upload()
		{
			var files = Request.Form.Files;

			var jsonFile = files.FirstOrDefault(f => f.Name == "json");
			if (jsonFile == null) return BadRequest(new UploadResponse { Success = false, Code = "JsonMissing" });

			var imageFile = files.FirstOrDefault(f => f.Name == "thumbnail");
			if (imageFile == null) return BadRequest(new UploadResponse { Success = false, Code = "ThumbnailMissing" });

			Guid projectId;
			using (var projectStream = await CopyToMemoryStream(jsonFile))
			{
				var tags = _projectParser.GetTagsFromProject(projectStream.ToArray());

				projectId = await _repository.CreateSceneAsync(tags);
				await _storage.SaveScene(projectId, projectStream);
				using (var imageStream = imageFile.OpenReadStream())
					await _storage.SaveSceneThumb(projectId, imageStream);
			}

			return Ok(new UploadResponse { Success = true, Id = projectId.ToString() });
		}

		private static async Task<MemoryStream> CopyToMemoryStream(IFormFile file)
		{
			var projectBytes = new MemoryStream();
			using (var stream = file.OpenReadStream())
			{
				await stream.CopyToAsync(projectBytes);
			}
			projectBytes.Seek(0, SeekOrigin.Begin);
			return projectBytes;
		}

		public class UploadResponse
		{
			public bool Success { get;set; }
			public string Code { get; set; }
			public string Id { get; set; }
		}
	}
}
