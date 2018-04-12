using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;

namespace VamBooru.Controllers
{
	public static class ControllerExtensions
	{
		public static UserLoginInfo GetUserLoginInfo(this Controller controller)
		{
			var user = controller.User;
			return new UserLoginInfo
			{
				//TODO: Get this from the logged in information
				Scheme = user.Identity.AuthenticationType,
				NameIdentifier = user.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value
			};
		}
	}
}