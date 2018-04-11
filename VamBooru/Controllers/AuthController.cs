using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace VamBooru.Controllers
{
	[Route("auth")]
	public class AuthController : Controller
	{
		private readonly string _scheme;

		public AuthController(IConfiguration configuration)
		{
			_scheme = configuration["Authentication:Scheme"] ?? throw new ArgumentException("Scheme was not configured", nameof(configuration));
		}

		[HttpGet("login")]
		public Task<IActionResult> Login([FromRoute] string scheme, [FromQuery] string returnUrl)
		{
			return _scheme == "AnonymousGuest"
				? AnonymousGuestLogin()
				: Task.FromResult(HandleOAuth());
		}

		[HttpGet("{scheme}/callback")]
		public IActionResult Callback([FromRoute] string scheme, [FromQuery] string returnUrl)
		{
			return HandleOAuth(returnUrl);
		}

		private IActionResult HandleOAuth(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties {RedirectUri = returnUrl});
		}

		public async Task<IActionResult> AnonymousGuestLogin(string returnUrl = "/")
		{
			var guestId = Guid.NewGuid().ToString();
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, $"anon-{guestId}"),
				new Claim(ClaimTypes.Name, $"anon-{guestId}"),
				new Claim("FullName", $"Anonymous User ({guestId})"),
				new Claim(ClaimTypes.Role, "AnonymousGuest"),
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
