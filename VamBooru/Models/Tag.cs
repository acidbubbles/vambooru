using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class Tag
	{
		public Guid Id { get; set; }
		[Required] public string Name { get; set; }
		public ICollection<PostTag> Scenes { get; set; } = new List<PostTag>();

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
