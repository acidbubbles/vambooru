using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class Tag
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public List<SceneTag> Scenes { get; set; }
	}
}