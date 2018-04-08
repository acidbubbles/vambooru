using System;
using System.Threading.Tasks;
using VamBooru.Models;

namespace VamBooru.Services
{
	public interface IRepository
	{
		Task<Scene[]> BrowseScenesAsync();
		Task<Guid> CreateSceneAsync(string[] tags);
	}
}