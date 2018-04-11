using System;
using System.Collections.Generic;

namespace VamBooru.Models
{
	public class User
	{
		public Guid Id { get; set; }
		public string Username { get; set; }
		public DateTimeOffset DateSubscribed { get; set; } = DateTimeOffset.UtcNow;
		public ICollection<Post> Scenes { get; set; } = new List<Post>();
		public ICollection<UserLogin> Logins { get; set; } = new List<UserLogin>();

		public AuthorViewModel ToViewModel()
		{
			return new AuthorViewModel
			{
				Username = Username
			};
		}
	}

	public class AuthorViewModel
	{
		public string Username { get; set; }
	}
}
