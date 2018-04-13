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

		public async Task<SceneFile> SaveSceneAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream)
		{
			using (var fileStream = File.OpenWrite(BuildJsonPath(sceneId)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public async Task<SceneFile> SaveSceneThumbAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream)
		{
			using (var fileStream = File.OpenWrite(BuildThumbPath(sceneId)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public async Task<SceneFile> LoadSceneThumbAsync(Guid sceneId)
		{
			var bytes = await File.ReadAllBytesAsync(BuildThumbPath(sceneId));
			return new SceneFile {Bytes = bytes};
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
