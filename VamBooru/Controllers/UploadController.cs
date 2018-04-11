using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/upload")]
	public class UploadController : Controller
	{
		private readonly IRepository _repository;
		private readonly IStorage _storage;
		private readonly ISceneParser _sceneParser;

		public UploadController(IRepository repository, IStorage storage, ISceneParser sceneParser)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			_sceneParser = sceneParser ?? throw new ArgumentNullException(nameof(sceneParser));
		}

		[HttpPost("")]
		[DisableFormValueModelBinding]
		public async Task<IActionResult> Upload()
		{
			var files = Request.Form.Files;

			var sceneJsonFile = files.FirstOrDefault(f => f.Name == "json");
			if (sceneJsonFile == null) return BadRequest(new UploadResponse { Success = false, Code = "JsonMissing" });

			var sceneJpgFile = files.FirstOrDefault(f => f.Name == "jpg");
			if (sceneJpgFile == null) return BadRequest(new UploadResponse { Success = false, Code = "ThumbnailMissing" });

			Guid postId;
			using (var sceneJsonStream = await CopyToMemoryStream(sceneJsonFile))
			using (var sceneJpgStream = await CopyToMemoryStream(sceneJpgFile))
			{
				var title = GuessTitle(sceneJsonFile);
				var tags = _sceneParser.GetTags(sceneJsonStream.ToArray());
				if (!ValidateJpeg(sceneJpgStream)) BadRequest(new UploadResponse {Success = false, Code = "InvalidJpegHeader"});

				var scene = new Scene {FilenameWithoutExtension = title};
				var post = await _repository.CreatePostAsync(title, tags, new[] {scene});
				postId = post.Id;

				await _storage.SaveSceneAsync(scene.Id, sceneJsonStream);
				await _storage.SaveSceneThumbAsync(scene.Id, sceneJpgStream);
			}

			return Ok(new UploadResponse { Success = true, Id = postId.ToString() });
		}

		private static bool ValidateJpeg(Stream sceneJpgStream)
		{
			if (!HasJpegHeader(sceneJpgStream))
				return false;

			sceneJpgStream.Seek(0, SeekOrigin.Begin);
			return true;
		}

		private static string GuessTitle(IFormFile jsonFile)
		{
			var title = jsonFile.FileName;
			if (string.IsNullOrEmpty(title))
				title = "untitled";
			else if (title.EndsWith(".json"))
				title = title.Substring(0, title.Length - ".json".Length);
			return title;
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

		private static bool HasJpegHeader(Stream stream)
		{
			using (var br = new BinaryReader(stream, Encoding.Default, true))
			{
				var soi = br.ReadUInt16();  // Start of Image (SOI) marker (FFD8)
				var marker = br.ReadUInt16(); // JFIF marker (FFE0) or EXIF marker(FF01)

				return soi == 0xd8ff && (marker & 0xe0ff) == 0xe0ff;
			}
		}

		public class UploadResponse
		{
			public bool Success { get; set; }
			public string Code { get; set; }
			public string Id { get; set; }
		}
	}
}
