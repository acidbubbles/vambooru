using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace VamBooru.Controllers
{
	[Route("auth")]
	public class AuthenticationController : Controller
	{
		[HttpGet("oauth2/login")]
		public IActionResult Login(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties { RedirectUri = returnUrl });
		}

		[HttpGet("fake/login")]
		public async Task<IActionResult> LoginFake(string returnUrl = "/")
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Name, "john.doe@localhost"),
				new Claim("FullName", "John Doe"),
				new Claim(ClaimTypes.Role, "Administrator"),
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			var authProperties = new AuthenticationProperties
			{
				RedirectUri = Url.Content(returnUrl)
			};

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(claimsIdentity),
				authProperties
			);

			return new ChallengeResult(authProperties);
		}
	}
}
