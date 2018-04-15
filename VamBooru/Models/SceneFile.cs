using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class SceneFile
	{
		public long Id { get; set; }
		[Required] public string Filename { get; set; }
		[Required] public string Extension { get; set; }
		[Required] public byte[] Bytes { get; set; }
		[Required] public Scene Scene { get; set; }

		public SceneFileViewModel ToViewModel()
		{
			return new SceneFileViewModel
			{
				Filename = Filename
			};
		}
	}

	public class SceneFileViewModel
	{
		public string Filename { get; set; }
	}
}
