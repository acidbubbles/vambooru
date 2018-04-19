using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class UserLogin : UserLoginInfo
	{
		[Required] public User User { get; set; }
	}
}
