using System;
using System.IO;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Storage
{
	public interface IStorage
	{
		Task<SceneFile> SaveSceneAsync(Scene scene, MemoryStream stream);
		Task<SceneFile> SaveSceneThumbAsync(Scene scene, MemoryStream stream);
		Task<SupportFile> SaveSupportFileAsync(Post post, string filename, MemoryStream stream);
		Task<Stream> LoadSceneThumbStreamAsync(Guid sceneId);
		Task<Stream> LoadSceneFileStreamAsync(Guid sceneId, string filename);
		Task<Stream> LoadSupportFileStreamAsync(Guid postId, string filename);
	}
}
