using System.ComponentModel.DataAnnotations;
using VamBooru.ViewModels;

namespace VamBooru.Models
{
	public class UserLogin : UserLoginInfo
	{
		[Required] public User User { get; set; }
	}
}
