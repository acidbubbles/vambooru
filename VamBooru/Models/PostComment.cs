using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VamBooru.Models
{
	public class PostComment
	{
		public long Id { get;set; }
		[Required] public Post Post { get; set; }
		[Required] public User Author { get;set; }
		public DateTimeOffset DateCreated { get; set; }
		[Required] [Column(TypeName = "text")] public string Text { get; set; }

		public override string ToString()
		{
			return $"{nameof(PostComment)} '{Text}')";
		}
	}
}
