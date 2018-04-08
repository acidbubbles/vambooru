using System;
using System.IO;
using System.Threading.Tasks;

namespace VamBooru.Services
{
	public interface IStorage
	{
		Task<string> SaveSceneAsync(Guid sceneId, Stream stream);
		Task<string> SaveSceneThumbAsync(Guid sceneId, Stream stream);
		Task<Stream> LoadSceneThumbAsync(Guid sceneId);
	}
}