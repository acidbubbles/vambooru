using Newtonsoft.Json;
using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class SceneViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		[JsonIgnore] public string ThumbnailUrn { get; set; }
		public string ThumbnailUrl { get; set; }

		public static SceneViewModel From(Scene from)
		{
			if(from == null) return null;

			return new SceneViewModel
			{
				Id = from.Id.ToString(),
				Name = from.Name,
				ThumbnailUrn = from.ThumbnailUrn
			};
		}
	}
}
