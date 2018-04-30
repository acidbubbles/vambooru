using System;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class PostTag
	{
		public Guid PostId { get; set; }
		[Required] public Post Post { get; set; }

		public Guid TagId { get; set; }
		[Required] public Tag Tag { get; set; }

		public override string ToString()
		{
			return $"{nameof(PostTag)} '{Tag.Name}'";
		}
	}
}
