using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VamBooru.Models;

namespace VamBooru.Services
{
	public class FileSystemStorage : IStorage
	{
		private readonly string _outputFolder;

		public FileSystemStorage(IConfiguration configuration)
		{
			var outputFolder = configuration?["Storage:FileSystem:Path"] ?? throw new ArgumentException("ProjectsPath was not defined", nameof(configuration));
			outputFolder = Environment.ExpandEnvironmentVariables(outputFolder);
			_outputFolder = outputFolder;
		}

		public async Task<SceneFile> SaveSceneAsync(Scene scene, MemoryStream stream)
		{
			using (var fileStream = File.OpenWrite(BuildJsonPath(scene.Id)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public async Task<SceneFile> SaveSceneThumbAsync(Scene scene, MemoryStream stream)
		{
			using (var fileStream = File.OpenWrite(BuildThumbPath(scene.Id)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public Task<Stream> LoadSceneThumbStreamAsync(Guid sceneId)
		{
			return Task.FromResult<Stream>(File.OpenRead(BuildThumbPath(sceneId)));
		}

		public Task<Stream> LoadSceneFileStreamAsync(Guid sceneId, string filename)
		{
			throw new NotImplementedException("Not implemented for File System Storage");
		}

		private string BuildJsonPath(Guid sceneId)
		{
			var filename = $"{sceneId}.json";
			var path = Path.Combine(_outputFolder, filename);
			return path;
		}

		private string BuildThumbPath(Guid sceneId)
		{
			var filename = $"{sceneId}.jpg";
			var path = Path.Combine(_outputFolder, filename);
			return path;
		}
	}
}
