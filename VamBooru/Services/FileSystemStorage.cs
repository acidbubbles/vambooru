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

		public async Task<string> SaveScene(Guid sceneId, Stream stream)
		{
			var filename = $"{sceneId}.json";
			var path = Path.Combine(_outputFolder, filename);
			using (var fileStream = File.OpenWrite(path))
				await stream.CopyToAsync(fileStream);
			return null;
		}

		public async Task<string> SaveSceneThumb(Guid sceneId, Stream stream)
		{
			var filename = $"{sceneId}.jpg";
			var path = Path.Combine(_outputFolder, filename);
			using (var fileStream = File.OpenWrite(path))
				await stream.CopyToAsync(fileStream);
			return null;
		}
	}
}
