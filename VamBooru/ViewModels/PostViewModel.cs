using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class PostViewModel
	{
		public string Id { get; set; }
		public bool Published { get; set; }
		public DateTimeOffset DatePublished { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		[JsonIgnore] public string ThumbnailUrn { get; set; }
		public string ThumbnailUrl { get; set; }
		public string DownloadUrl { get; set; }
		public int Version { get; set; }
		public int Votes { get; set; }
		public TagViewModel[] Tags { get; set; }
		public UserViewModel Author { get; set; }
		public SceneViewModel[] Scenes { get; set; }
		public FileViewModel[] Files { get; set; }
		
		public static PostViewModel From(Post from, bool optimize)
		{
			if(from == null) return null;

			var tags = from.Tags ?? new List<PostTag>();
			return new PostViewModel
			{
				Id = from.Id.ToString(),
				Published = from.Published,
				DatePublished = from.DatePublished ?? from.DateCreated,
				Title = from.Title,
				Text = from.Text,
				ThumbnailUrn = from.ThumbnailUrn,
				Version = from.Version,
				Votes = from.Votes,
				Tags = tags.Select(tag => TagViewModel.From(tag.Tag)).OrderByDescending(t => t.PostsCount).ThenBy(t => t.Name).ToArray(),
				Author = UserViewModel.From(from.Author),
				Scenes = optimize ? null : (from.Scenes?.OrderBy(s => s.Name).Select(SceneViewModel.From).ToArray() ?? new SceneViewModel[0]),
				Files = from.PostFiles?.OrderBy(pf => pf.Filename).Select(FileViewModel.From).ToArray()
			};
		}
	}
}
