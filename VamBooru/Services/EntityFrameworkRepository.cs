using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Services
{
	public class EntityFrameworkRepository : IRepository
	{
		private readonly VamBooruDbContext _context;

		public EntityFrameworkRepository(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<Guid> CreateSceneAsync(string title, string[] tags)
		{
			var scene = new Scene {Title = title};
			//TODO: Assign tags
			_context.Scenes.Add(scene);
			await _context.SaveChangesAsync();
			return scene.Id;
		}

		public Task<Scene> LoadSceneAsync(Guid id)
		{
			return _context.Scenes.FindAsync(id);
		}

		public Task<Scene[]> BrowseScenesAsync()
		{
			return _context.Scenes.Where(s => s.Published).ToArrayAsync();
		}

		public async Task UpdateSceneAsync(Scene scene)
		{
			//TODO: Validate input ID, owner, etc.
			var dbScene = await _context.Scenes.FindAsync(scene.Id);
			dbScene.Title = scene.Title;
			dbScene.Published = scene.Published;
			await _context.SaveChangesAsync();
		}
	}
}