using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class Scene
	{
		public Guid Id { get; set; }
		public string FilenameWithoutExtension { get; set; }
		public Post Post { get; set; }
		public ICollection<SceneFile> Files { get; set; }

		public SceneViewModel ToViewModel()
		{
			return new SceneViewModel
			{
				Id = Id.ToString(),
			};
		}
	}

	public class SceneViewModel
	{
		public string Id { get; set; }
	}
}
