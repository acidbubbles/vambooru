using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.Storage;

namespace VamBooru.Controllers
{
	[Route("api/download")]
	public class DownloadController : Controller
	{
		private readonly IPostsRepository _postsRepository;
		private readonly IPostFilesRepository _filesRepository;
		private readonly IStorage _storage;

		public DownloadController(IPostsRepository postsRepository, IPostFilesRepository filesRepository, IStorage storage)
		{
			_postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
			_filesRepository = filesRepository ?? throw new ArgumentNullException(nameof(filesRepository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("posts/{postId}", Name = nameof(DownloadPostAsync))]
		public async Task<IActionResult> DownloadPostAsync([FromRoute] Guid postId)
		{
			var post = await _postsRepository.LoadPostAsync(postId);
			var files = await _filesRepository.LoadPostFilesAsync(postId);

			//TODO: This should be done way before
			var username = SanitizingUtils.GetSanitizedFilename(post.Author.Username);

			//TODO: We should generate the zip upfront and store it for direct download
			var zipStream = new MemoryStream();
			using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
			{
				foreach (var file in files)
				{
					var filename = SanitizingUtils.GetSanitizedFilename(file.Filename);
					using(var stream = await _storage.LoadFileStreamAsync(file.Urn, file.Compressed))
					{
						if (stream == null)
							throw new Exception($"The file {file.Urn} was missing from post {postId}");

					var entry = zip.CreateEntry($"scenes/{username}/{filename}");
						using (var entryStream = entry.Open())
						{
							await stream.CopyToAsync(entryStream);
						}
					}
				}
			}

			zipStream.Seek(0, SeekOrigin.Begin);

			return File(zipStream, "application/octet-stream", $"{username} - {SanitizingUtils.GetSanitizedFilename(post.Title)}.zip");
		}
	}
}
