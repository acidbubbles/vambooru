using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Services
{
	public class EntityFrameworkStorage : IStorage
	{
		private readonly VamBooruDbContext _context;

		public EntityFrameworkStorage(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<SceneFile> SaveSceneAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream)
		{
			return await SaveSceneFile(filenameWithoutExtension, stream, ".json", true);
		}

		public async Task<SceneFile> SaveSceneThumbAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream)
		{
			return await SaveSceneFile(filenameWithoutExtension, stream, ".jpg", false);
		}

		private async Task<SceneFile> SaveSceneFile(string filenameWithoutExtension, MemoryStream stream, string extension, bool compressed)
		{
			if (compressed)
			{
				var result = new MemoryStream();
				using (var gzip = new GZipStream(result, CompressionLevel.Optimal, true))
				{
					stream.CopyTo(gzip);
				}
				stream = result;
			}

			var file = new SceneFile
			{
				Filename = filenameWithoutExtension + extension,
				Bytes = stream.ToArray()
			};
			_context.SceneFiles.Add(file);
			await _context.SaveChangesAsync();
			return file;
		}

		public Task<SceneFile> LoadSceneThumbAsync(Guid sceneId)
		{
			var file = _context.SceneFiles.FirstOrDefaultAsync(sf => sf.Scene.Id == sceneId);
			return file;
		}
	}
}

