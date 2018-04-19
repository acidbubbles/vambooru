using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Storage;

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

		[HttpGet("posts/{postId}", Name = nameof(DownloadPostAsync))]
		public async Task<IActionResult> DownloadPostAsync([FromRoute] Guid postId)
		{
			var post = await _repository.LoadPostAsync(postId);
			var files = await _repository.LoadPostFilesAsync(postId, true);

			//TODO: This should be done way before
			var username = GetSanitizedFilename(post.Author.Username);

			//TODO: We should generate the zip upfront and store it for direct download
			var zipStream = new MemoryStream();
			using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
			{
				foreach (var file in files)
				{
					var filename = GetSanitizedFilename(file.Filename);
					var entry = zip.CreateEntry($"scenes/{username}/{filename}");
					Stream stream = null;
					try
					{
						switch (file)
						{
							case SceneFile sf:
								stream = await _storage.LoadSceneFileStreamAsync(sf.Scene.Id, sf.Filename);
								if (stream == null)
									throw new Exception($"The file {sf.Filename} was missing from scene {sf.Scene.Id} in post {postId}");
								break;
							case SupportFile sf:
								stream = await _storage.LoadSupportFileStreamAsync(sf.Post.Id, sf.Filename);
								if (stream == null)
									throw new Exception($"The file {sf.Filename} was missing from post {postId}");
								break;
							default:
								throw new Exception($"Unknown file type: {file.GetType()}");
						}

						using (var entryStream = entry.Open())
						{
							await stream.CopyToAsync(entryStream);
						}
					}
					finally
					{
						stream?.Dispose();
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
