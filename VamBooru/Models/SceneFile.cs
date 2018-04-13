namespace VamBooru.Models
{
	public class SceneFile
	{
		public long Id { get; set; }
		public string Filename { get; set; }
		public byte[] Bytes { get; set; }
		public Scene Scene { get; set; }
	}
}
