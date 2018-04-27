using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VamBooru.Repository;

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

		[HttpGet("{scheme}/login")]
		[ResponseCache(NoStore = true, Duration = 0)]
		public async Task<IActionResult> Login([FromRoute] string scheme)
		{
			if (_defaultScheme == "AnonymousGuest")
				return await AnonymousGuestLoginAsync();

			await HttpContext.ChallengeAsync(_defaultScheme, new AuthenticationProperties {RedirectUri = Url.RouteUrl(nameof(ValidateLogin))});
			return new EmptyResult();
		}

		[HttpGet("logout")]
		[ResponseCache(NoStore = true, Duration = 0)]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			return Redirect("/");
		}

		[HttpGet("login/validate", Name = nameof(ValidateLogin))]
		[ResponseCache(NoStore = true, Duration = 0)]
		public async Task<IActionResult> ValidateLogin()
		{
			if (!User.Identity.IsAuthenticated)
				return Redirect("/");

			var scheme = User.Identity.AuthenticationType;
			var identifier = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new Exception($"There was no identifier for the logged in user with scheme '{scheme}'");
			var uid = GetUniqueUsername("user");
			var username = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value ?? $"anon.{uid}";

			//TODO: This should be replaced by a signup page when the user does not already exist
			//TODO: Retry more than once if conflict
			var result = await _repository.LoadOrCreateUserFromLoginAsync(scheme, identifier, username, DateTimeOffset.UtcNow);

			switch (result.Result)
			{
				case LoadOrCreateUserFromLoginResultTypes.NewUser:
					return Redirect("/welcome");
				case LoadOrCreateUserFromLoginResultTypes.ExistingUser:
					return Redirect($"/users/{result.Login.User.Username}");
				default:
					return Redirect("/");
			}
		}

		public async Task<IActionResult> AnonymousGuestLoginAsync()
		{
			// This is a method used for development only

			var userId = GetUniqueUsername("anon");
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Name, userId),
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

		private static string GetUniqueUsername(string prefix)
		{
			return prefix + "." + Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
		}
	}
}
