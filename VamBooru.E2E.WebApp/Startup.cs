using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VamBooru.E2E.WebApp.Bootstrapping;
using VamBooru.Models;

namespace VamBooru.E2E.WebApp
{
	public class Startup : VamBooru.Startup
	{
		public Startup(IConfiguration configuration) : base(configuration)
		{
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
