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
using VamBooru.Repository;
using VamBooru.Storage;
using VamBooru.VamFormat;

namespace VamBooru.Controllers
{
	[Route("api/upload")]
	public class UploadController : Controller
	{
		private const int MaxFileSize = 5 * 1000 * 1000; // 5MB

		private readonly IUsersRepository _usersRepository;
		private readonly IPostsRepository _postsRepository;
		private readonly IStorage _storage;
		private readonly ISceneFormat _sceneFormat;

		public UploadController(IUsersRepository usersRepository, IPostsRepository postsRepository, IStorage storage, ISceneFormat sceneFormat)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			_sceneFormat = sceneFormat ?? throw new ArgumentNullException(nameof(sceneFormat));
		}

		[HttpPost("")]
		[DisableFormValueModelBinding]
		[Authorize]
		public async Task<IActionResult> Upload()
		{
			//TODO: Refactor this; it's way too complicated.
			var user = await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo());
			if (user == null) return Unauthorized();

			if(!OrganizeFiles(out var scenesFiles, out var supportFiles, out var errorCode)) return BadRequest(new UploadResponse {Success = false, Code = errorCode});

			var scenes = await Task.WhenAll(scenesFiles.Select(async sf =>
				new Tuple<Scene, SceneJsonAndJpg, MemoryStream, MemoryStream>(
					new Scene {Name = sf.FilenameWithoutExtension},
					sf,
					await CopyToMemoryStream(sf.JsonFile),
					await CopyToMemoryStream(sf.JpgFile)
				)));

			var tags = new List<string>();
			foreach (var sceneData in scenes)
			{
				var project = _sceneFormat.Deserialize(sceneData.Item3.ToArray());
				tags.AddRange(_sceneFormat.GetTags(project));
				if (!ValidateJpeg(sceneData.Item4)) BadRequest(new UploadResponse {Success = false, Code = "InvalidJpegHeader"});
			}

			var postFiles = new List<PostFile>();
			string postThumbnailUrn = null;
			foreach (var sceneData in scenes)
			{
				postFiles.Add(new PostFile
				{
					Urn = await _storage.SaveFileAsync(sceneData.Item3, true),
					Filename = sceneData.Item2.FilenameWithoutExtension + ".json",
					MimeType = "application/json",
					Compressed = true
				});
				sceneData.Item3.Dispose();
				var thumbUrn = await _storage.SaveFileAsync(sceneData.Item4, false);
				if (postThumbnailUrn == null) postThumbnailUrn = thumbUrn;
				postFiles.Add(new PostFile
				{
					Urn = thumbUrn,
					Filename = sceneData.Item2.FilenameWithoutExtension + ".jpg",
					MimeType = "image/jpeg",
					Compressed = false
				});
				sceneData.Item4.Dispose();
				sceneData.Item1.ThumbnailUrn = thumbUrn;
			}

			foreach (var supportFile in supportFiles)
			{
				var memoryStream = new MemoryStream();
				using (var supportFileStream = supportFile.OpenReadStream())
				{
					await supportFileStream.CopyToAsync(memoryStream);
				}
				memoryStream.Seek(0, SeekOrigin.Begin);

				postFiles.Add(new PostFile
				{
					Urn = await _storage.SaveFileAsync(memoryStream, true),
					Filename = supportFile.FileName,
					MimeType = MimeTypeUtils.Of(supportFile.FileName),
					Compressed = true
				});
			}

			var post = await _postsRepository.CreatePostAsync(
				this.GetUserLoginInfo(),
				scenes.First().Item1.Name,
				tags.Distinct().ToArray(),
				scenes.Select(s => s.Item1).ToArray(),
				postFiles.ToArray(),
				postThumbnailUrn,
				DateTimeOffset.UtcNow
			);

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
				if (file.Length > MaxFileSize)
				{
					errorCode = $"FileTooLarge:{MaxFileSize/1000/1000}MB";
					return true;
				}

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

				if (MimeTypeUtils.Known.Contains(extension))
				{
					//TODO: We should validate these files
					supportFiles.Add(file);
					continue;
				}

				errorCode = $"UnsupportedExtension:{extension}";
				return true;
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
