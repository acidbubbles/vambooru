using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class TagViewModel
	{
		public string Id { get;set; }
		public string Name { get; set; }
		public int PostsCount { get; set; }

		public static TagViewModel From(Tag from)
		{
			if(from == null) return null;

			return new TagViewModel
			{
				Id = from.Id.ToString(),
				Name = from.Name,
				PostsCount = from.PostsCount
			};
		}
	}
}
