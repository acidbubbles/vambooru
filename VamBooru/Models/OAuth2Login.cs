namespace VamBooru.Models
{
	public class OAuth2Login
	{
		public User User { get; set; }
		public string Scheme { get; set; }
		public string Username { get; set; }
	}
}
