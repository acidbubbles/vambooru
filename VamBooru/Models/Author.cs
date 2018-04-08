using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class Author
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public List<Scene> Scenes { get; set; }
	}
}