using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class Tag
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public ICollection<SceneTag> Scenes { get; set; } = new List<SceneTag>();

		public TagViewModel ToViewModel()
		{
			return new TagViewModel
			{
				Id = Id.ToString(),
				Name = Name
			};
		}
	}

	public class TagViewModel
	{
		public string Id { get;set; }
		public string Name { get; set; }
	}
}
