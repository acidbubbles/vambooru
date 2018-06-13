using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("auth")]
	public class AuthController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly string _defaultScheme;

		public AuthController(IConfiguration configuration, IUsersRepository usersRepository)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_defaultScheme = configuration["Authentication:Scheme"] ?? throw new ArgumentException("Scheme was not configured", nameof(configuration));
		}

		[HttpGet("{scheme}/login")]
		[ResponseCache(NoStore = true, Duration = 0)]
		public async Task<IActionResult> Login([FromRoute] string scheme)
		{
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

			var result = await _usersRepository.LoadOrCreateUserFromLoginAsync(scheme, identifier, username, DateTimeOffset.UtcNow);

			switch (result.Result)
			{
				case LoadOrCreateUserFromLoginResultTypes.NewUser:
					return Redirect("/welcome");
				case LoadOrCreateUserFromLoginResultTypes.ExistingUser:
					return Redirect("/me");
				default:
					return Redirect("/");
			}
		}

		private static string GetUniqueUsername(string prefix)
		{
			return prefix + "." + Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
		}
	}
}
