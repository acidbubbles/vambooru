using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("auth")]
	public class AuthController : Controller
	{
		private readonly IRepository _repository;
		private readonly string _defaultScheme;

		public AuthController(IConfiguration configuration, IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_defaultScheme = configuration["Authentication:Scheme"] ?? throw new ArgumentException("Scheme was not configured", nameof(configuration));
		}

		[HttpGet("login")]
		public async Task<IActionResult> Login()
		{
			if (_defaultScheme == "AnonymousGuest")
				return await AnonymousGuestLoginAsync();

			await HttpContext.ChallengeAsync(_defaultScheme, new AuthenticationProperties {RedirectUri = Url.RouteUrl(nameof(ValidateLogin))});
			return new EmptyResult();
		}

		[HttpGet("login/validate", Name = nameof(ValidateLogin))]
		public async Task<IActionResult> ValidateLogin()
		{
			if (!User.Identity.IsAuthenticated)
				return Redirect("/");

			var scheme = User.Identity.AuthenticationType;
			var id = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var name = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;

			//TODO: This should be replaced by a signup page when the user does not already exist
			await _repository.CreateUserFromLoginAsync(scheme, id, name);

			return Redirect("/");
		}

		public async Task<IActionResult> AnonymousGuestLoginAsync()
		{
			// This is a method used for development only

			var guestId = Guid.NewGuid().ToString();
			var userId = $"anon-{guestId}";
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Name, userId),
				new Claim("FullName", $"Anonymous User ({guestId})"),
				new Claim(ClaimTypes.Role, "AnonymousGuest"),
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

			var returnUrl = Url.RouteUrl(nameof(ValidateLogin));
			var authProperties = new AuthenticationProperties
			{
				RedirectUri = Url.Content(returnUrl)
			};

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(claimsIdentity),
				authProperties
			);

			return Redirect(returnUrl);
		}
	}
}
