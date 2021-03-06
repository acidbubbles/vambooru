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
		public UserRoles Role { get; set; }
		public ICollection<Post> Posts { get; set; }
		public ICollection<PostComment> PostComments { get; set; }
		public ICollection<UserLogin> Logins { get; set; }
		
		public override string ToString()
		{
			return $"{nameof(User)} '{Username}'";
		}
	}

	public enum UserRoles
	{
		Standard = 0,
		Moderator = 10,
		Admin = 100
	}
}
