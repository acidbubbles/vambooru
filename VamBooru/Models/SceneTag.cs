namespace VamBooru.Models
{
	public class SceneTag
	{
		public long Id { get; set; }
		public Scene Scene { get; set; }
		public Tag Tag { get; set; }
	}
}