using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
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
using VamBooru.Middlewares;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Storage;
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

		public void ConfigureServices(IServiceCollection services)
		{ 
			services.AddSingleton(Configuration);

			services.Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
				//NOTE: This allows someone to "fake" using HTTPS, but in practice we don't really care. We don't need a secure relationship with the proxy.
				options.KnownNetworks.Clear();
				options.KnownProxies.Clear();
			});

			services.AddResponseCompression();

			services
				.AddMvc(options =>
				{
					if (Configuration["Web:Https"] == "True")
						options.Filters.Add(new RequireHttpsAttribute());
				})
				.AddJsonOptions(options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; });

			ConfigureDatabase(services);

			ConfigureAngular(services);

			services.AddTransient<IRepository, EntityFrameworkRepository>();

			switch (Configuration["Storage:Type"])
			{
				case "EFPostgres":
					services.AddTransient<IStorage, EntityFrameworkStorage>();
					break;
					default:
						throw new Exception($"Unknown storage type: {Configuration["Storage:Type"]}");
			}

			services.AddTransient<ISceneFormat, JsonSceneFormat>();

			services.AddMemoryCache();

			ConfigureAuthentication(services);
		}

		private void ConfigureDatabase(IServiceCollection services)
		{
			services.AddEntityFrameworkNpgsql().AddDbContext<VamBooruDbContext>(options =>
			{
				options.UseNpgsql(
					Configuration["Repository:EFPostgres:ConnectionString"] ??
					throw new NullReferenceException("The VamBooru Postgres connection string was not configured in appsettings.json")
				);
			});
		}

		private static void ConfigureAngular(IServiceCollection services)
		{
			services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

			services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
		}

		private void ConfigureAuthentication(IServiceCollection services)
		{
			var authenticationScheme = Configuration["Authentication:Scheme"];

			switch (authenticationScheme)
			{
				case "AnonymousGuest":
					ConfigureAnonymousGuestAuthentication(services);
					break;
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

		private static void ConfigureAnonymousGuestAuthentication(IServiceCollection services)
		{
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options => options.LoginPath = "/");
		}

		private void ConfigureOAuthAuthentication(IServiceCollection services, string scheme, Action<OAuthOptions> configureOptions)
		{
			services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/";
			});

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
			app.UseForwardedHeaders();
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

			app.MapWhen(
				context => context.Request.Path.StartsWithSegments("/api"),
				appBuilder => appBuilder.Run(async c =>
				{
					c.Response.StatusCode = 404;
					c.Response.ContentType = "text/plain";
					await c.Response.WriteAsync("This route does not exist");
				})
			);

			app.UseSpa(spa =>
			{
				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});
		}
	}
}
