using System.ComponentModel.DataAnnotations;
using VamBooru.ViewModels;

namespace VamBooru.Models
{
	public class SceneFile
	{
		public long Id { get; set; }
		[Required] public string Filename { get; set; }
		[Required] public string Extension { get; set; }
		[Required] public byte[] Bytes { get; set; }
		[Required] public Scene Scene { get; set; }

		public FileViewModel ToViewModel()
		{
			return new FileViewModel
			{
				Filename = Filename
			};
		}
	}
}
