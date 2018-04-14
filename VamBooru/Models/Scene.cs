using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class Scene
	{
		public Guid Id { get; set; }
		[Required] public string Name { get; set; }
		[Required] public Post Post { get; set; }
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
