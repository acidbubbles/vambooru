using System;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class Scene
	{
		public Guid Id { get; set; }
		[Required] public string Name { get; set; }
		[Required] public Post Post { get; set; }
		public string ThumbnailUrn { get; set; }
	}
}
