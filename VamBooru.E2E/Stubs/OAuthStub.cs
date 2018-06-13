using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VamBooru.E2E.Stubs
{
	public class OAuthStub
	{
		public const string Scheme = "Stub";
	}

	public static class OAuthStubExtensions
	{
		public static AuthenticationBuilder AddOAuthStub(this AuthenticationBuilder builder, Action<CookieAuthenticationOptions> configureOptions)
		{
			return builder.AddScheme<CookieAuthenticationOptions, OAuthStubHandler>(OAuthStub.Scheme, "Authentication Stub (Tests Only)", configureOptions);
		}
	}

	internal class OAuthStubHandler : CookieAuthenticationHandler
	{
		public OAuthStubHandler(IOptionsMonitor<CookieAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}
	}
}
