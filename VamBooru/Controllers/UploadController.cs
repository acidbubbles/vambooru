using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
		[Authorize]
		public async Task<IActionResult> Upload()
		{
			if(!OrganizeFiles(out var scenesFiles, out var supportFiles, out var errorCode)) return BadRequest(new UploadResponse {Success = false, Code = errorCode});
			if (supportFiles.Any())
				return BadRequest(new UploadResponse {Success = false, Code = "SupportFilesNotYetSupported"});

			var scenes = await Task.WhenAll(scenesFiles.Select(async sf =>
				new Tuple<Scene, SceneJsonAndJpg, MemoryStream, MemoryStream>(
					new Scene {FilenameWithoutExtension = sf.FilenameWithoutExtension},
					sf,
					await CopyToMemoryStream(sf.JsonFile),
					await CopyToMemoryStream(sf.JpgFile)
				)));

			var tags = new List<string>();
			foreach (var sceneData in scenes)
			{
				tags.AddRange(_sceneParser.GetTags(sceneData.Item3.ToArray()));
				if (!ValidateJpeg(sceneData.Item4)) BadRequest(new UploadResponse {Success = false, Code = "InvalidJpegHeader"});
			}

			var post = await _repository.CreatePostAsync(this.GetUserLoginInfo(), scenes.First().Item1.FilenameWithoutExtension, tags.Distinct().ToArray(), scenes.Select(s => s.Item1).ToArray());

			foreach (var sceneData in scenes)
			{
				await _storage.SaveSceneAsync(sceneData.Item1.Id, sceneData.Item1.FilenameWithoutExtension, sceneData.Item3);
				await _storage.SaveSceneThumbAsync(sceneData.Item1.Id, sceneData.Item1.FilenameWithoutExtension, sceneData.Item4);
			}

			return Ok(new UploadResponse { Success = true, Id = post.Id.ToString() });
		}

		public bool OrganizeFiles(out List<SceneJsonAndJpg> scenesFiles, out List<IFormFile> supportFiles, out string errorCode)
		{
			var files = Request.Form.Files;

			supportFiles = new List<IFormFile>();
			scenesFiles = files
				.Where(file => Path.GetExtension(file.FileName) == ".json")
				.Select(file => new SceneJsonAndJpg
				{
					JsonFile = file,
					FilenameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName)
				})
				.ToList();

			if (!scenesFiles.Any())
			{
				errorCode = "NoSceneJson";
				return false;
			}

			foreach (var file in files)
			{
				var filename = file.FileName;
				if (string.IsNullOrEmpty(filename))
				{
					errorCode = "MissingFilename";
					return false;
				}

				var extension = Path.GetExtension(file.FileName);
				var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
				if (extension == ".json") continue;
				if (extension == ".jpg")
				{
					var sceneFile = scenesFiles.FirstOrDefault(sf => sf.FilenameWithoutExtension == filenameWithoutExtension);
					if (sceneFile != null)
					{
						sceneFile.JpgFile = file;
						continue;
					}
				}
				supportFiles.Add(file);
			}

			errorCode = null;
			return true;
		}

		private static bool ValidateJpeg(Stream sceneJpgStream)
		{
			if (!HasJpegHeader(sceneJpgStream))
				return false;

			sceneJpgStream.Seek(0, SeekOrigin.Begin);
			return true;
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

		public class SceneJsonAndJpg
		{
			public string FilenameWithoutExtension { get; set; }
			public IFormFile JsonFile { get; set; }
			public IFormFile JpgFile { get; set; }
		}

		public class UploadResponse
		{
			public bool Success { get; set; }
			public string Code { get; set; }
			public string Id { get; set; }
		}
	}
}
