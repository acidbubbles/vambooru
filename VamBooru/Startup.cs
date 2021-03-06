using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using StackExchange.Redis;
using VamBooru.Middlewares;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;
using VamBooru.Storage.EFPostgres;
using VamBooru.VamFormat;

namespace VamBooru
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		private SecureHeadersMiddlewareConfiguration _secureHeadersMiddlewareConfiguration;

		public void ConfigureServices(IServiceCollection services)
		{ 
			services.AddSingleton(Configuration);
			services.AddMemoryCache();
			services.AddResponseCompression();

			services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/";
			});

			ConfigureOwasp();
			ConfigureReverseProxy(services);
			ConfigureRateLimit(services);
			ConfigureDataProtection(services);
			ConfigureMvc(services);
			ConfigureDatabase(services);
			ConfigureAngular(services);
			ConfigureRepository(services);
			ConfigureStorage(services);
			ConfigureVamBooruServices(services);
			ConfigureAuthentication(services);

			OnConfigurationComplete(services);
		}

		protected virtual void OnConfigurationComplete(IServiceCollection services)
		{
		}

		private void ConfigureOwasp()
		{
			var config = SecureHeadersMiddlewareBuilder.CreateBuilder()
				.UseXFrameOptions()
				.UseXSSProtection()
				.UseContentTypeOptions()
				.UsePermittedCrossDomainPolicies()
				.UseReferrerPolicy()
				.RemovePoweredByHeader();

			if (Configuration["Web:Https"] == "True")
				config = config
					.UseHsts()
					.UseContentDefaultSecurityPolicy()
					.UseExpectCt("https://vambooru.herokuapp.com/api/security/report", 86400, true);

			_secureHeadersMiddlewareConfiguration = config.Build();
		}

		private static void ConfigureReverseProxy(IServiceCollection services)
		{
			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
				//NOTE: This allows someone to "fake" using HTTPS, but in practice we don't really care. We don't need a secure relationship with the proxy.
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();
			});
		}

		private void ConfigureRateLimit(IServiceCollection services)
		{
			services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

			services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
		}

		private static void ConfigureVamBooruServices(IServiceCollection services)
		{
			services.AddTransient<ISceneFormat, JsonSceneFormat>();
		}

		private void ConfigureMvc(IServiceCollection services)
		{
			services
				.AddMvc(options =>
				{
					if (Configuration["Web:Https"] == "True")
						options.Filters.Add(new RequireHttpsAttribute());
				})
				.AddJsonOptions(options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; });
		}

		private void ConfigureDataProtection(IServiceCollection services)
		{
			var redisUrl = Configuration["DataProtection:Redis:Url"];
			if (redisUrl.StartsWith("$"))
			{
				redisUrl = Environment.GetEnvironmentVariable(redisUrl.Substring(1));
				if (redisUrl == null) return;
				var match = Regex.Match(redisUrl, @"redis://(?<username>.+?):(?<password>.+?)@(?<server>.+?):(?<port>\d+)");
				if (!match.Success) throw new Exception("Invalid redis environment variable: regex did not match");
				redisUrl = $"{match.Groups["server"]}:{match.Groups["port"]},password={match.Groups["password"]}";
			}

			if (string.IsNullOrEmpty(redisUrl)) return;
			var redis = ConnectionMultiplexer.Connect(redisUrl);
			services.AddDataProtection().PersistKeysToRedis(redis, "DataProtection-Keys");
		}

		private void ConfigureRepository(IServiceCollection services)
		{
			switch (Configuration["Repository:Type"])
			{
				case "EFPostgres":
					services.AddEFPostgresRepository();
					break;
				default:
					throw new Exception($"Unknown repository type: {Configuration["Repository:Type"]}");
			}
		}

		private void ConfigureStorage(IServiceCollection services)
		{
			switch (Configuration["Storage:Type"])
			{
				case "EFPostgres":
					services.AddEFPostgresStorage();
					break;
				default:
					throw new Exception($"Unknown storage type: {Configuration["Storage:Type"]}");
			}
		}

		private void ConfigureDatabase(IServiceCollection services)
		{
			var connectionString = Configuration["Repository:EFPostgres:ConnectionString"] ?? throw new NullReferenceException( "The VamBooru Postgres connection string was not configured in appsettings.json");

			if (connectionString.StartsWith("$"))
			{
				connectionString = Environment.GetEnvironmentVariable(connectionString.Substring(1));
				if (connectionString == null) throw new NullReferenceException( "The VamBooru Postgres connection string points to an environment variable that does not exist");
				var match = Regex.Match(connectionString, @"postgres://(?<username>.+?):(?<password>.+?)@(?<server>.+?):(?<port>\d+)/(?<database_name>.+)");
				if (!match.Success) throw new Exception("Invalid VamBooru Postgres environment variable: regex did not match");
				connectionString = $"Server={match.Groups["server"]};Port={match.Groups["port"]};Database={match.Groups["database_name"]};User Id={match.Groups["username"]};Password={match.Groups["password"]};";
			}
			services.AddEntityFrameworkNpgsql().AddDbContext<VamBooruDbContext>(options =>
			{
				options.UseNpgsql(connectionString);
			});
		}

		private static void ConfigureAngular(IServiceCollection services)
		{
			services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

			services.AddSpaStaticFiles(configuration =>
			{
			 	configuration.RootPath = "ClientApp/dist";
			});
		}

		protected virtual void ConfigureAuthentication(IServiceCollection services)
		{
			var authenticationScheme = Configuration["Authentication:Scheme"];

			switch (authenticationScheme)
			{
				case "GitHub":
					ConfigureGitHubAuthentication(services);
					break;
				default:
					throw new Exception("Invalid or missing authentication scheme");
			}
		}

		private void ConfigureGitHubAuthentication(IServiceCollection services)
		{
			ConfigureOAuthAuthentication(services, "GitHub", options =>
			{
				options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
				options.TokenEndpoint = "https://github.com/login/oauth/access_token";
				options.UserInformationEndpoint = "https://api.github.com/user";

				options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
				options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
				options.ClaimActions.MapJsonKey("urn:github:login", "login");
				options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
				options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
			});
		}

		private void ConfigureOAuthAuthentication(IServiceCollection services, string scheme, Action<OAuthOptions> configureOptions)
		{
			services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = scheme;
				})
				.AddCookie(options => options.LoginPath = "/")
				.AddOAuth(scheme, options =>
				{
					options.ClientId = Configuration[$"Authentication:{scheme}:ClientId"];
					options.ClientSecret = Configuration[$"Authentication:{scheme}:ClientSecret"];
					options.CallbackPath = new PathString($"/auth/{scheme.ToLower()}/callback");

					configureOptions(options);

					options.Events = new OAuthEvents
					{
						OnCreatingTicket = async context =>
						{
							var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
							request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
							request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

							var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
							response.EnsureSuccessStatusCode();

							var user = JObject.Parse(await response.Content.ReadAsStringAsync());

							context.RunClaimActions(user);
						}
					};
				});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			OnBeforeConfigure(app, env);

			app.UseForwardedHeaders();
			app.UseIpRateLimiting();
			app.UseSecureHeadersMiddleware(_secureHeadersMiddlewareConfiguration);
			app.UseResponseCompression();

			app.UseExceptionHandler(new ExceptionHandlerOptions
			{
				ExceptionHandler = new JsonExceptionMiddleware(env).Invoke
			});

			if (Configuration["Web:Https"] == "True")
			{
				app.UseRewriter(new RewriteOptions().AddRedirectToHttps());
			}

			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action=Index}/{id?}");
			});

			app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"), Return404);
			app.MapWhen(context => context.Request.Path.StartsWithSegments("/auth"), Return404);

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = Path.Combine(env.ContentRootPath, "ClientApp");

				if (env.IsDevelopment())
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});
		}

		protected virtual void OnBeforeConfigure(IApplicationBuilder app, IHostingEnvironment env)
		{
		}

		private static void Return404(IApplicationBuilder app)
		{
			app.Run(async c =>
			{
				c.Response.StatusCode = 404;
				c.Response.ContentType = "text/plain";
				await c.Response.WriteAsync("This route does not exist");
			});
		}
	}
}

