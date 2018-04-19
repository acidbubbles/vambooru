namespace VamBooru.ViewModels
{
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
