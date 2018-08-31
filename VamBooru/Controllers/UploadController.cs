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
		//TODO: This class is absolutely not tested

		private const int MaxFileSize = 20 * 1000 * 1000; // 20MB

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

		[HttpPost("posts/{postId}")]
		[DisableFormValueModelBinding]
		[Authorize]
		public async Task<IActionResult> UploadToExistingPost([FromRoute] Guid postId)
		{
			//TODO: Updating a scene may result in broken zip downloads. We should disable the post for the duration of the update.

			var user = await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo());
			if (user == null) return Unauthorized();

			var post = await _postsRepository.LoadPostAsync(postId);
			if (post.Author.Id != user.Id) return Unauthorized();

			// Save files and prepare template
			Post postTemplate;
			try
			{
				postTemplate = await UploadAndCreatePostTemplate();
			}
			catch (PostUploadException exc)
			{
				return BadRequest(new UploadResponse { Success = false, Code = exc.Code });
			}

			// Temporarily unpublish
			var published = post.Published;
			post.Published = false;
			await _postsRepository.SavePostAsync(post);

			// Delete old files and scenes, save the new ones
			var filesToDelete = post.PostFiles.Select(f => f.Urn).ToArray();
			await _postsRepository.UpdatePostScenesAndFiles(post, postTemplate.Scenes.ToArray(), postTemplate.PostFiles.ToArray());
			foreach (var fileToDelete in filesToDelete)
			{
				await _storage.DeleteFileAsync(fileToDelete);
			}

			// Republish and increment version
			post.Published = published;
			post.ThumbnailUrn = post.Scenes.FirstOrDefault()?.ThumbnailUrn;
			post.Version++;
			post.DatePublished = DateTimeOffset.UtcNow;
			await _postsRepository.SavePostAsync(post);

			return Ok(new UploadResponse { Success = true, Id = post.Id.ToString() });
		}

		[HttpPost("")]
		[DisableFormValueModelBinding]
		[Authorize]
		public async Task<IActionResult> UploadAndCreatePost()
		{
			var user = await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo());
			if (user == null) return Unauthorized();

			Post postTemplate;
			try
			{
				postTemplate = await UploadAndCreatePostTemplate();
			}
			catch (PostUploadException exc)
			{
				return BadRequest(new UploadResponse { Success = false, Code = exc.Code });
			}

			var post = await _postsRepository.CreatePostAsync(
				this.GetUserLoginInfo(),
				postTemplate,
				DateTimeOffset.UtcNow
			);

			return Ok(new UploadResponse { Success = true, Id = post.Id.ToString() });
		}

		private async Task<Post> UploadAndCreatePostTemplate()
		{
			//TODO: Refactor this; it's way too complicated.

			OrganizeFiles(out var scenesFiles, out var supportFiles);

			var scenes = await Task.WhenAll(scenesFiles.Select(async sf =>
				new Tuple<Scene, SceneAndThumbnail, MemoryStream, MemoryStream>(
					new Scene {Name = sf.FilenameWithoutExtension},
					sf,
					await CopyToMemoryStream(sf.SceneFile),
					await CopyToMemoryStream(sf.JpgFile)
				)));

			var tags = new List<string>();
			foreach (var sceneData in scenes)
			{
				if (sceneData.Item2.SceneFile.FileName.EndsWith(".json"))
				{
					var project = _sceneFormat.Deserialize(sceneData.Item3.ToArray());
					tags.AddRange(_sceneFormat.GetTags(project));
				}

				if (!ValidateJpeg(sceneData.Item4))
					throw new PostUploadException("InvalidJpegHeader", $"The file '{sceneData.Item2.FilenameWithoutExtension}.jpg' is not a valid jpeg image.");
			}

			var postFiles = new List<PostFile>();
			string postThumbnailUrn = null;
			foreach (var sceneData in scenes)
			{
				postFiles.Add(new PostFile
				{
					Urn = await _storage.SaveFileAsync(sceneData.Item3, true),
					Filename = sceneData.Item2.SceneFile.FileName,
					MimeType = MimeTypeUtils.Of(sceneData.Item2.SceneFile.FileName),
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

			return new Post
			{
				Title = scenes.First().Item1.Name,
				Tags = tags.Distinct().Select(t => new PostTag {Tag = new Tag {Name = t}}).ToArray(),
				Scenes = scenes.Select(s => s.Item1).ToArray(),
				PostFiles = postFiles.ToArray(),
				ThumbnailUrn = postThumbnailUrn,
				Version = 1
			};
		}

		public void OrganizeFiles(out List<SceneAndThumbnail> scenesFiles, out List<IFormFile> supportFiles)
		{
			var files = Request.Form.Files;

			if(files.Count == 0)
				throw new PostUploadException("NoFiles", "There was no files in the upload");

			supportFiles = new List<IFormFile>();
			scenesFiles = files
				.Where(file => new[] { ".json", ".vac" }.Contains(Path.GetExtension(file.FileName)))
				.Select(file => new SceneAndThumbnail
				{
					SceneFile = file,
					FilenameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName)
				})
				.ToList();

			if (!scenesFiles.Any())
				throw new PostUploadException("NoSceneJson", "There was no .json or .vac file in the uploaded files list");

			foreach (var file in files)
			{
				if (file.Length <= 0)
					throw new PostUploadException("FileEmpty", $"File {file.FileName} is empty.");

				if (file.Length > MaxFileSize)
					throw new PostUploadException("FileTooLarge", $"File {file.FileName} is too large. Size: {file.Length / 1000 / 1000} Max: {MaxFileSize / 1000 / 1000}MB");

				var filename = file.FileName;
				if (string.IsNullOrEmpty(filename))
					throw new PostUploadException("MissingFilename", "File has no filename.");

				var extension = Path.GetExtension(file.FileName);
				var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
				switch (extension)
				{
					case ".json":
					case ".vac":
						continue;
					case ".jpg":
						var sceneFile = scenesFiles.FirstOrDefault(sf => sf.FilenameWithoutExtension == filenameWithoutExtension);
						if (sceneFile != null)
						{
							sceneFile.JpgFile = file;
							continue;
						}

						break;
				}

				if (!MimeTypeUtils.Known.Contains(extension))
					throw new PostUploadException("UnsupportedExtension", $"Unsupported file extension: '{file.FileName}'");

				//TODO: We should validate these files
				supportFiles.Add(file);
			}
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

		public class SceneAndThumbnail
		{
			public string FilenameWithoutExtension { get; set; }
			public IFormFile SceneFile { get; set; }
			public IFormFile JpgFile { get; set; }
		}

		public class UploadResponse
		{
			public bool Success { get; set; }
			public string Code { get; set; }
			public string Id { get; set; }
		}
	}

	public class PostUploadException : Exception
	{
		public string Code { get; set; }

		public PostUploadException(string code, string message)
			: base(message)
		{
			Code = code;
		}
	}
}
