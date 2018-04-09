using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IRepository
	{
		Task<Guid> CreateSceneAsync(string title, string[] tags);
		Task<Scene> LoadSceneAsync(Guid id);
		Task<Scene[]> BrowseScenesAsync(int page, int pageSize);
		Task UpdateSceneAsync(SceneViewModel scene);
	}
}