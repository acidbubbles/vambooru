using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class PostFile
	{
		public long Id { get; set; }
		[Required] public string Urn { get; set; }
		[Required] public string Filename { get; set; }
		public bool Compressed { get; set; }
		[Required] public string MimeType { get; set; }
		[Required] public Post Post { get; set; }

		public override string ToString()
		{
			return $"{nameof(PostFile)} '{Filename}' ({MimeType}{(Compressed ? " gzip" : "")})";
		}
	}
}
