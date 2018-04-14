using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace VamBooru.Models
{
	public class Post
	{
		public Guid Id { get; set; }
		public bool Published { get; set; }
		public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset DatePublished { get; set; }
		[Required] public string Title { get; set; }
		[Required] [Column(TypeName = "text")] public string Text { get; set; }
		public string ImageUrl { get; set; }
		[Required] public User Author { get; set; }
		public int Votes { get; set; }
		public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
		public ICollection<Scene> Scenes { get; set; } = new List<Scene>();
		public ICollection<UserPostVote> UserVotes { get; set; } = new List<UserPostVote>();

		public PostViewModel ToViewModel(bool optimize)
		{
			return new PostViewModel
			{
				Id = Id.ToString(),
				Published = Published,
				Title = Title,
				Text = Text,
				ImageUrl = ImageUrl,
				Tags = Tags?.Select(tag => tag.Tag.ToViewModel()).OrderBy(t => t.Name).ToArray(),
				Author = Author?.ToViewModel()
			};
		}
	}

	public class PostViewModel
	{
		public string Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		public string ImageUrl { get; set; }
		public TagViewModel[] Tags { get; set; }
		public UserViewModel Author { get; set; }
	}
}
