namespace VamBooru.Models
{
	public class UserLogin : UserLoginInfo
	{
		public User User { get; set; }
	}

	public class UserLoginInfo
	{
		public string Scheme { get; set; }
		public string NameIdentifier { get; set; }
	}
}
