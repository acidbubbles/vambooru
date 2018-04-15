using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/download")]
	public class DownloadController : Controller
	{
		private readonly IRepository _repository;
		private readonly IStorage _storage;

		private static readonly Regex InvalidFilenameCharactersRegex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled, TimeSpan.FromSeconds(1));

		public DownloadController(IRepository repository, IStorage storage)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("posts/{postId}")]
		public async Task<IActionResult> DownloadPost([FromRoute] Guid postId)
		{
			var post = await _repository.LoadPostFilesAsync(postId);

			//TODO: This should be done way before
			var username = GetSanitizedFilename(post.Author.Username);

			//TODO: We should generate the zip upfront
			var zipStream = new MemoryStream();
			using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
			{
				foreach (var file in post.Scenes.SelectMany(s => s.Files))
				{
					var filename = GetSanitizedFilename(file.Filename);
					var entry = zip.CreateEntry($"scenes/{username}/{filename}");
					using (var entryStream = entry.Open())
					{
						using (var content = await _storage.LoadSceneFileStreamAsync(file.Scene.Id, file.Filename))
							await content.CopyToAsync(entryStream);
					}
				}
			}

			zipStream.Seek(0, SeekOrigin.Begin);

			return File(zipStream, "application/octet-stream", $"{username}-{post.Id}.zip");
		}

		private static string GetSanitizedFilename(string filename)
		{
			var result = InvalidFilenameCharactersRegex.Replace(filename, "");
			if (string.IsNullOrWhiteSpace(result)) throw new Exception("Invalid username");
			return result;
		}
	}
}
