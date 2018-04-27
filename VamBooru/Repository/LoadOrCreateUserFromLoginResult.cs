using VamBooru.Models;

namespace VamBooru.Repository
{
	public class LoadOrCreateUserFromLoginResult
	{
		public UserLogin Login { get; set; }
		public LoadOrCreateUserFromLoginResultTypes Result { get; set; }
	}

	public enum LoadOrCreateUserFromLoginResultTypes
	{
		NewUser,
		ExistingUser,
	}
}
