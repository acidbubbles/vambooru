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
			var dbScene = await LoadSceneAsync(Guid.Parse(scene.Id));
			dbScene.Title = scene.Title;
			dbScene.Published = scene.Published;
			await AssignTagsAsync(dbScene, scene.Tags.Select(tag => tag.Name).ToArray());
			await _context.SaveChangesAsync();
		}

		private async Task AssignTagsAsync(Scene scene, string[] tags)
		{
			// Remove tags
			foreach (var tag in scene.Tags.ToArray())
			{
				if (!tags.Contains(tag.Tag.Name))
					scene.Tags.Remove(tag);
			}

			// Add tags
			var currentTags = scene.Tags.Select(t => t.Tag.Name).ToArray();
			var newTags = tags.Where(t => !currentTags.Contains(t)).ToArray();

			foreach (var newTag in await GetOrCreateTagsAsync(newTags))
			{
				var sceneTag = new SceneTag { Scene = scene, Tag = newTag };
				_context.SceneTags.Add(sceneTag);
				scene.Tags.Add(sceneTag);
			}
		}

		private async Task<Tag[]> GetOrCreateTagsAsync(string[] tags)
		{
			if (!(tags?.Any() ?? false)) return new Tag[0];

			var dbTags = new List<Tag>();

			//TODO: This is not "thread safe", multiple users could create the same tags at the same time
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

			return dbTags.ToArray();
		}
	}
}

