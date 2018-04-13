using System;
using System.IO;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IStorage
	{
		Task<SceneFile> SaveSceneAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream);
		Task<SceneFile> SaveSceneThumbAsync(Guid sceneId, string filenameWithoutExtension, MemoryStream stream);
		Task<SceneFile> LoadSceneThumbAsync(Guid sceneId);
	}
}
