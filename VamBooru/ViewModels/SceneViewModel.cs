using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class SceneViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string ImageUrl { get; set; }

		public static SceneViewModel From(Scene from)
		{
			if(from == null) return null;

			return new SceneViewModel
			{
				Id = from.Id.ToString(),
				Name = from.Name
			};
		}
	}
}
