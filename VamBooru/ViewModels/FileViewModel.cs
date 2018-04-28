using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class FileViewModel
	{
		public string Filename { get; set; }

		public static FileViewModel From(PostFile from)
		{
			if(from == null) return null;

			return new FileViewModel
			{
				Filename = from.Filename
			};
		}
	}
}
