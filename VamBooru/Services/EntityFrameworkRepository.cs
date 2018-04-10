using System;
using System.Collections.Generic;
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
			var dbTags = await GetOrCreateTags(tags);
			var scene = new Scene
			{
				Title = title,
				Tags = dbTags.Select(t => new SceneTag {Tag = t}).ToList()
			};
			_context.Scenes.Add(scene);
			await _context.SaveChangesAsync();
			return scene.Id;
		}

		private async Task<Tag[]> GetOrCreateTags(string[] tags)
		{
			var dbTags = new List<Tag>();

			if (tags?.Any() ?? false)
			{
				foreach (var tag in tags)
				{
					var dbTag = await _context.Tags.SingleOrDefaultAsync(t => t.Name == tag);
					if (dbTag == null)
					{
						dbTag = new Tag {Name = tag};
						_context.Tags.Add(dbTag);
					}
					dbTags.Add(dbTag);
				}
			}

			return dbTags.ToArray();
		}

		public Task<Scene> LoadSceneAsync(Guid id)
		{
			return _context.Scenes.FindAsync(id);
		}

		public Task<Scene[]> BrowseScenesAsync(int page, int pageSize)
		{
			return _context.Scenes.AsNoTracking().Where(s => s.Published).Skip(page * pageSize).Take(pageSize).ToArrayAsync();
		}

		public async Task UpdateSceneAsync(SceneViewModel scene)
		{
			//TODO: Validate input ID, owner, etc.
			var dbScene = await _context.Scenes.FindAsync(Guid.Parse(scene.Id));
			dbScene.Title = scene.Title;
			dbScene.Published = scene.Published;
			await _context.SaveChangesAsync();
		}
	}
}
