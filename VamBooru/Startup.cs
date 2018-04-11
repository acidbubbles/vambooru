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
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VamBooru.Models;
using VamBooru.Services;

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
			services.AddMvc()
				.AddJsonOptions(options => { options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; });

			ConfigureDatabase(services);

			ConfigureAngular(services);

			services.AddTransient<IRepository, EntityFrameworkRepository>();
			services.AddTransient<IStorage, FileSystemStorage>();
			services.AddTransient<ISceneParser, JsonSceneParser>();

			ConfigureAuthentication(services);
		}

		private void ConfigureDatabase(IServiceCollection services)
		{
			services.AddEntityFrameworkNpgsql().AddDbContext<VamBooruDbContext>(options =>
			{
				options.UseNpgsql(
					Configuration.GetConnectionString("VamBooru") ??
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
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/error");
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
