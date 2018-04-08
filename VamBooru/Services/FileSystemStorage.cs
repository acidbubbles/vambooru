using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VamBooru.Services
{
	public class FileSystemStorage : IStorage
	{
		private readonly string _outputFolder;

		public FileSystemStorage(IConfiguration configuration)
		{
			_outputFolder = configuration?["VamBooru:ProjectsPath"] ?? throw new ArgumentException("ProjectsPath was not defined", nameof(configuration));
		}

		public async Task<string> SaveSceneAsync(Guid sceneId, Stream stream)
		{
			using (var fileStream = File.OpenWrite(BuildJsonPath(sceneId)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public async Task<string> SaveSceneThumbAsync(Guid sceneId, Stream stream)
		{
			using (var fileStream = File.OpenWrite(BuildThumbPath(sceneId)))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public Task<Stream> LoadSceneThumbAsync(Guid sceneId)
		{
			return Task.FromResult<Stream>(new FileStream(BuildThumbPath(sceneId), FileMode.Open, FileAccess.Read));
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
