using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class SupportFile
	{
		public long Id { get; set; }
		[Required] public string Filename { get; set; }
		[Required] public byte[] Bytes { get; set; }
		[Required] public Post Post { get; set; }
	}
}
