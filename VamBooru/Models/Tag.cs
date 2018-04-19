using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class Tag
	{
		public Guid Id { get; set; }
		[Required] public string Name { get; set; }
		[Description("Calculated when a tag is added or removed")] public int PostsCount { get; set; }
	}
}
