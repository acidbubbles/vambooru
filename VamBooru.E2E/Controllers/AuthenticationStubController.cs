using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VamBooru.E2E.Stubs;

namespace VamBooru.E2E.Controllers
{
	[Route("auth/Stub")]
	public class AuthenticationStubController : Controller
	{
		private readonly ILogger<AuthenticationStubController> _logger;
		private static string CreateUniqueId() => Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");

		public AuthenticationStubController(ILogger<AuthenticationStubController> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		[HttpGet("authenticate")]
		public async Task<IActionResult> Root([FromQuery] string id, [FromQuery] string returnUrl)
		{
			var nameIdentifier = id ?? $"{OAuthStub.Scheme}.{CreateUniqueId()}";

			var identity = new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
				new Claim(ClaimTypes.Name, $"{nameIdentifier} {DateTimeOffset.UtcNow}"),
			}, OAuthStub.Scheme);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				new ClaimsPrincipal(identity),
				new AuthenticationProperties
				{
					RedirectUri = Url.Content(returnUrl)
				}
			);

			_logger.LogInformation($"Authenticed using {OAuthStub.Scheme}: {nameIdentifier}");
			return Redirect(returnUrl);
		}
	}
}
