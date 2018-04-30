using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VamBooru.Models
{
	public class Post
	{
		public Guid Id { get; set; }
		public bool Published { get; set; }
		public DateTimeOffset? DatePublished { get; set; }
		public DateTimeOffset DateCreated { get; set; }
		[Required] public string Title { get; set; }
		[Required] [Column(TypeName = "text")] public string Text { get; set; }
		[Required] public User Author { get; set; }
		public string ThumbnailUrn { get; set; }
		[Description("Calculated when UserVotes is updated")] public int Votes { get; set; }
		public ICollection<PostTag> Tags { get; set; }
		public ICollection<Scene> Scenes { get; set; }
		public ICollection<PostFile> PostFiles { get; set; }
		public ICollection<UserPostVote> UserVotes { get; set; }
		public ICollection<PostComment> Comments { get; set; }
		
		public override string ToString()
		{
			return $"{nameof(Post)} '{Title}'";
		}
	}
}
