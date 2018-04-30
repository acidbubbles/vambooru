using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class TagViewModel
	{
		public string Name { get; set; }
		public int PostsCount { get; set; }

		public static TagViewModel From(Tag from)
		{
			if(from == null) return null;

			return new TagViewModel
			{
				Name = from.Name,
				PostsCount = from.PostsCount
			};
		}
	}
}
