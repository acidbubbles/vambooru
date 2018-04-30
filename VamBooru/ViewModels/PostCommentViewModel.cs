using System;
using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class PostCommentViewModel
	{
		public string Text { get; set; }
		public UserViewModel Author { get; set; }
		public DateTimeOffset DateCreated { get; set; }

		public static PostCommentViewModel From(PostComment from)
		{
			if(from == null) return null;

			return new PostCommentViewModel
			{
				DateCreated = from.DateCreated,
				Author = UserViewModel.From(from.Author),
				Text = from.Text
			};
		}
	}
}