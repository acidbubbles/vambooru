using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class UserLogin : UserLoginInfo
	{
		[Required] public User User { get; set; }
	}

	public class UserLoginInfo
	{
		public string Scheme { get; set; }
		public string NameIdentifier { get; set; }

		public UserLoginInfo()
		{
		}

		public UserLoginInfo(string scheme, string nameIdentifier)
		{
			Scheme = scheme;
			NameIdentifier = nameIdentifier;
		}
	}
}
