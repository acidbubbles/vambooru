using System;
using System.Collections.Generic;
using System.Linq;

namespace VamBooru.Models
{
	public class Post
	{
		public Guid Id { get; set; }
		public bool Published { get; set; }
		public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
		public DateTimeOffset DatePublished { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public ICollection<PostTag> Tags { get; set; } = new List<PostTag>();
		public ICollection<Scene> Scenes { get; set; } = new List<Scene>();
		public User Author { get; set; }

		public PostViewModel ToViewModel()
		{
			return new PostViewModel
			{
				Id = Id.ToString(),
				Published = Published,
				Title = Title,
				ImageUrl = ImageUrl,
				Tags = Tags?.Select(tag => tag.Tag?.ToViewModel()).ToArray(),
				Author = Author?.ToViewModel()
			};
		}
	}

	public class PostViewModel
	{
		public string Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public TagViewModel[] Tags { get; set; }
		public AuthorViewModel Author { get; set; }
	}
}
