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

		public Task<Scene[]> BrowseScenesAsync()
		{
			return _context.Scenes.Where(s => s.Published).ToArrayAsync();
		}

		public async Task<Guid> CreateSceneAsync(string[] tags)
		{
			var scene = new Scene();
			//TODO: Assign tags
			_context.Scenes.Add(scene);
			await _context.SaveChangesAsync();
			return scene.Id;
		}
	}
}