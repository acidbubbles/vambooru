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
		private readonly string _scheme;

		public AuthController(IConfiguration configuration, IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_scheme = configuration["Authentication:Scheme"] ?? throw new ArgumentException("Scheme was not configured", nameof(configuration));
		}

		[HttpGet("login")]
		public async Task<IActionResult> Login([FromRoute] string scheme, [FromQuery] string returnUrl)
		{
			if (_scheme == "AnonymousGuest")
				await AnonymousGuestLoginAsync();
			else
				await HandleOAuthAsync();

			await CreateUserAsync(_scheme);
			return Redirect("/");
		}

		[HttpGet("{scheme}/callback")]
		public Task Callback([FromRoute] string scheme, [FromQuery] string returnUrl)
		{
			return HandleOAuthAsync(returnUrl);
		}

		private async Task HandleOAuthAsync(string returnUrl = "/")
		{
			await HttpContext.ChallengeAsync(new AuthenticationProperties { RedirectUri = returnUrl });
		}

		public async Task AnonymousGuestLoginAsync(string returnUrl = "/")
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

			HttpContext.User = new ClaimsPrincipal(new[] {new ClaimsIdentity(claims)});
		}

		private async Task CreateUserAsync(string scheme)
		{
			var id = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
			var name = User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;

			await _repository.CreateUserFromLoginAsync(scheme, id, name);
		}
	}
}
