using System;
using System.IO;
using System.Threading.Tasks;

namespace VamBooru.Services
{
	public interface IStorage
	{
		Task<string> SaveScene(Guid sceneId, Stream stream);
		Task<string> SaveSceneThumb(Guid sceneId, Stream stream);
	}
}