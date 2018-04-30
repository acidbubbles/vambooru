using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class UserLogin
	{
		[Required] public User User { get; set; }
		[Required] public string Scheme { get; set; }
		[Required] public string NameIdentifier { get; set; }
		
		public override string ToString()
		{
			return $"{nameof(UserLogin)} {Scheme} '{NameIdentifier}'";
		}
	}
}
