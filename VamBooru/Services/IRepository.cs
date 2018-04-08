using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IRepository
	{
		Task<Guid> CreateSceneAsync(string title, string[] tags);
		Task<Scene> LoadSceneAsync(Guid id);
		Task<Scene[]> BrowseScenesAsync();
		Task UpdateSceneAsync(Scene scene);
	}
}