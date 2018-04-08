using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class Scene
	{
		public Guid Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public List<SceneTag> Tags { get; set; }
		public Author Author { get; set; }
	}
}
