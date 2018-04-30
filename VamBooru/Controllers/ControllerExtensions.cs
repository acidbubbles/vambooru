using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	public static class ControllerExtensions
	{
		public static UserLoginInfo GetUserLoginInfo(this Controller controller)
		{
			var user = controller.User;
			return new UserLoginInfo
			{
				Scheme = user.Identity.AuthenticationType,
				NameIdentifier = user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value
			};
		}
	}
}
