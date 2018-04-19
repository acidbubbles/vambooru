using VamBooru.Models;

namespace VamBooru.ViewModels
{
	public class UserViewModel
	{
		public string Username { get; set; }

		public static UserViewModel From(User from)
		{
			if(from == null) return null;

			return new UserViewModel
			{
				Username = from.Username
			};
		}
	}
}
