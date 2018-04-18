using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Storage
{
	public class EntityFrameworkStorage : IStorage
	{
		private const string JpgExtension = ".jpg";
		private const string JsonExtension = ".json";

		private readonly VamBooruDbContext _context;

		public EntityFrameworkStorage(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<SceneFile> SaveSceneAsync(Scene scene, MemoryStream stream)
		{
			return await SaveSceneFile(scene, stream, JsonExtension, true);
		}

		public async Task<SceneFile> SaveSceneThumbAsync(Scene scene, MemoryStream stream)
		{
			return await SaveSceneFile(scene, stream, JpgExtension, false);
		}

		private async Task<SceneFile> SaveSceneFile(Scene scene, MemoryStream stream, string extension, bool compressed)
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
				Scene = scene,
				Filename = scene.Name + extension,
				Extension = extension,
				Bytes = stream.ToArray()
			};
			_context.SceneFiles.Add(file);
			await _context.SaveChangesAsync();
			return file;
		}

		public async Task<Stream> LoadSceneFileStreamAsync(Guid sceneId, string filename)
		{
			var file = await _context.SceneFiles.FirstOrDefaultAsync(sf => sf.Scene.Id == sceneId && sf.Filename == filename);
			if (file == null) return null;
			return new MemoryStream(file.Bytes);
		}

		public async Task<Stream> LoadSceneThumbStreamAsync(Guid sceneId)
		{
			var file = await _context.SceneFiles.FirstOrDefaultAsync(sf => sf.Scene.Id == sceneId && sf.Extension == JpgExtension);
			if (file == null) return null;
			return new MemoryStream(file.Bytes);
		}
	}
}

