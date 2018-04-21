using System.Linq;
using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class PostViewModel
	{
		public string Id { get; set; }
		public bool Published { get; set; }
		public string Title { get; set; }
		public string Text { get; set; }
		public string ImageUrl { get; set; }
		public string DownloadUrl { get; set; }
		public int Votes { get; set; }
		public TagViewModel[] Tags { get; set; }
		public UserViewModel Author { get; set; }
		public SceneViewModel[] Scenes { get; set; }
		public FileViewModel[] Files { get; set; }
		
		public static PostViewModel From(Post from, bool optimize)
		{
			if(from == null) return null;

			return new PostViewModel
			{
				Id = from.Id.ToString(),
				Published = from.Published,
				Title = from.Title,
				Text = from.Text,
				ImageUrl = from.ImageUrl,
				Votes = from.Votes,
				Tags = from.Tags?.Select(tag => TagViewModel.From(tag.Tag)).OrderByDescending(t => t.PostsCount).ThenBy(t => t.Name).ToArray(),
				Author = UserViewModel.From(from.Author),
				Scenes = optimize ? null : from.Scenes.Select(SceneViewModel.From).ToArray()
			};
		}
	}
}
