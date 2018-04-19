using System.Collections.Generic;

namespace VamBooru.Models
{
	public class TagViewModel
	{
		public string Id { get;set; }
		public string Name { get; set; }
		public ICollection<PostTag> Posts { get; set; } = new List<PostTag>();

		public static TagViewModel From(Tag from)
		{
			if(from == null) return null;

			return new TagViewModel
			{
				Id = from.Id.ToString(),
				Name = from.Name
			};
		}
	}
}
