using System;
using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class UserPostVote
	{
		public Guid PostId { get; set; }
		[Required] public Post Post { get; set; }

		public Guid UserId { get; set; }
		[Required] public User User { get; set; }

		public int Votes { get; set; }

		public override string ToString()
		{
			return $"{nameof(UserPostVote)} {Votes} votes";
		}
	}
}
