using System;

namespace VamBooru.Models
{
	public class PostTag
	{
		public Guid PostId { get; set; }
		public Post Post { get; set; }

		public Guid TagId { get; set; }
		public Tag Tag { get; set; }
	}
}
