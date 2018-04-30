using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Storage;

namespace VamBooru.Controllers
{
	[Route("/api/admin")]
	public class AdminController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly VamBooruDbContext _dbContext;
		private readonly IStorage _storage;

		public AdminController(IUsersRepository usersRepository, VamBooruDbContext dbContext, IStorage storage)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
		}

		[HttpGet("migrations/ExtractStorage")]
		public async Task<IActionResult> MigrationsExtractStorage()
		{
			if ((await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo())).Role != UserRoles.Admin)
				return Unauthorized();

#pragma warning disable 612
			var result = new List<string>();

			{
				var sceneFiles = await _dbContext.SceneFiles.Include(sf => sf.Scene).ThenInclude(s => s.Post).ToArrayAsync();
				foreach (var sceneFile in sceneFiles)
				{
					var compressed = sceneFile.Extension == ".json";
					var mimeType = MimeTypeUtils.Of(sceneFile.Extension);
					var urn = await _storage.SaveFileAsync(new MemoryStream(sceneFile.Bytes), compressed);
					result.Add($"Scene {sceneFile.Scene.Id} File {sceneFile.Id} saved to {urn} in post {sceneFile.Scene.Post}");

					if (sceneFile.Extension == ".jpg")
					{
						sceneFile.Scene.ThumbnailUrn = urn;
						if (sceneFile.Scene.Post.ThumbnailUrn == null) sceneFile.Scene.Post.ThumbnailUrn = urn;
					}
					_dbContext.PostFiles.Add(new PostFile
					{
						Compressed = compressed,
						Filename = sceneFile.Filename,
						MimeType = mimeType,
						Post = sceneFile.Scene.Post,
						Urn = urn
					});

					_dbContext.SceneFiles.Remove(sceneFile);

					await _dbContext.SaveChangesAsync();
				}
			}

			{
				var supportFiles = await _dbContext.SupportFiles.Include(sf => sf.Post).ToArrayAsync();
				foreach (var supportFile in supportFiles)
				{
					var mimeType = MimeTypeUtils.Of(Path.GetExtension(supportFile.Filename));
					var urn = await _storage.SaveFileAsync(new MemoryStream(supportFile.Bytes), false);
					result.Add($"Post {supportFile.Post.Id} File {supportFile.Id} saved to {urn}");

					_dbContext.PostFiles.Add(new PostFile
					{
						Compressed = false,
						Filename = supportFile.Filename,
						MimeType = mimeType,
						Post = supportFile.Post,
						Urn = urn
					});

					_dbContext.SupportFiles.Remove(supportFile);

					await _dbContext.SaveChangesAsync();
				}
			}

			return Ok(result);
#pragma warning restore 612
		}
	}
}
