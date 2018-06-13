using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VamBooru.E2E.Bootstrapping;
using VamBooru.E2E.Stubs;
using VamBooru.Models;

namespace VamBooru.E2E.Web
{
	public class TestStartup : Startup
	{
		public TestStartup(IConfiguration configuration) : base(configuration)
		{
		}

		protected override void ConfigureAuthentication(IServiceCollection services)
		{
			if (Configuration["Authentication:Scheme"] != "Stub")
			{
				base.ConfigureAuthentication(services);
				return;
			}

			services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = OAuthStub.Scheme;
				})
				.AddOAuthStub(options => { options.LoginPath = "/auth/Stub/authenticate"; })
				.AddCookie(options => options.LoginPath = "/auth/Stub/authenticate");
		}

		protected override void OnConfigurationComplete(IServiceCollection services)
		{
			services.AddTransient<Bootstrapper>();
		}

		protected override void OnBeforeConfigure(IApplicationBuilder app, IHostingEnvironment env)
		{
			using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			using (var dbContext = serviceScope.ServiceProvider.GetService<VamBooruDbContext>())
			{
				dbContext.Database.OpenConnection();
				dbContext.Database.EnsureCreated();

				var bootstrapper = serviceScope.ServiceProvider.GetService<Bootstrapper>();
				bootstrapper.Seed().Wait();
			}
		}
	}
}
