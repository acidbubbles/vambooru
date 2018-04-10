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
			var scene = new Scene
			{
				Title = title,
			};
			await AssignTagsAsync(scene, tags);

			_context.Scenes.Add(scene);
			await _context.SaveChangesAsync();
			return scene.Id;
		}

		public Task<Scene> LoadSceneAsync(Guid id)
		{
			return _context.Scenes
				.Include(s => s.Author)
				.Include(s => s.Tags)
				.ThenInclude(t => t.Tag)
				.FirstOrDefaultAsync(s => s.Id == id);
		}

		public Task<Scene[]> BrowseScenesAsync(int page, int pageSize)
		{
			return _context.Scenes
				.AsNoTracking()
				.Include(s => s.Author)
				.Include(s => s.Tags)
				.ThenInclude(t => t.Tag)
				.Where(s => s.Published)
				.Skip(page * pageSize)
				.Take(pageSize)
				.ToArrayAsync();
		}

		public async Task UpdateSceneAsync(SceneViewModel scene)
		{
			//TODO: Validate input ID, owner, etc.
			var dbScene = await _context.Scenes.FindAsync(Guid.Parse(scene.Id));
			dbScene.Title = scene.Title;
			dbScene.Published = scene.Published;
			await AssignTagsAsync(dbScene, scene.Tags.Select(tag => tag.Name).ToArray());
			await _context.SaveChangesAsync();
		}

		private async Task AssignTagsAsync(Scene scene, string[] tags)
		{
			//TODO: The implementation of this method could be greatly improved (verify for existing GUID, load multiple items at once, etc.)
			var dbTags = await GetOrCreateTagsAsync(tags);

			var sceneTags = dbTags.Select(t =>
			{
				var sceneTag = new SceneTag { Scene = scene, Tag = t };
				_context.SceneTags.Add(sceneTag);
				return sceneTag;
			}).ToList();

			scene.Tags = sceneTags;
		}

		private async Task<Tag[]> GetOrCreateTagsAsync(string[] tags)
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
	}
}
