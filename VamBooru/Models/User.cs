using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class User
	{
		public Guid Id { get; set; }
		[Required] public string Username { get; set; }
		public DateTimeOffset DateSubscribed { get; set; }
		public ICollection<Post> Scenes { get; set; } = new List<Post>();
		public ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();
	}
}
