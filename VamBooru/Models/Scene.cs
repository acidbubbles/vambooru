using System;
using System.Collections.Generic;
using System.Linq;

namespace VamBooru.Models
{
	public class Scene
	{
		public Guid Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public ICollection<SceneTag> Tags { get; set; } = new List<SceneTag>();
		public Author Author { get; set; }

		public SceneViewModel ToViewModel()
		{
			return new SceneViewModel
			{
				Id = Id.ToString(),
				Published = Published,
				Title = Title,
				ImageUrl = ImageUrl,
				Tags = Tags.Select(tag => tag.Tag.ToViewModel()).ToArray()
			};
		}
	}

	public class SceneViewModel
	{
		public string Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public TagViewModel[] Tags { get; set; }
	}
}
