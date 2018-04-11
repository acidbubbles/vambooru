using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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

			services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/dist";
			});

			services.AddEntityFrameworkNpgsql().AddDbContext<VamBooruDbContext>(options =>
			{
				options.UseNpgsql(Configuration.GetConnectionString("VamBooru") ?? throw new NullReferenceException("The VamBooru Postgres connection string was not configured in appsettings.json"));
			});

			services.AddTransient<IRepository, EntityFrameworkRepository>();
			services.AddTransient<IStorage, FileSystemStorage>();
			services.AddTransient<IProjectParser, JsonProjectParser>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();
			app.UseSpaStaticFiles();

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
