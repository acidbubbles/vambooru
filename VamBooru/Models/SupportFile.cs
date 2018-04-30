using System;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	[Obsolete]
	public class SupportFile
	{
		public long Id { get; set; }
		[Required] public string Filename { get; set; }
		[Required] public byte[] Bytes { get; set; }
		[Required] public Post Post { get; set; }
	}
}
