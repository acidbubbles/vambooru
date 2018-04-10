using System;

namespace VamBooru.Models
{
	public class SceneTag
	{
		public Guid SceneId { get; set; }
		public Scene Scene { get; set; }

		public Guid TagId { get; set; }
		public Tag Tag { get; set; }
	}
}
