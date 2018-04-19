using System;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class Tag
	{
		public Guid Id { get; set; }
		[Required] public string Name { get; set; }
	}
}
