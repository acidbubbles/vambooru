using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
			services.AddMvc();
			services.AddSingleton(Configuration);

			var connectionString = Configuration.GetConnectionString("VamBooru") ?? throw new NullReferenceException("The VamBooru connection string was not configured in appsettings.json");
			services.AddDbContext<VamBooruDbContext>(options =>
			{
				options.UseSqlServer(connectionString);
			});

			services.AddTransient<IRepository, EntityFrameworkRepository>();
			services.AddTransient<IStorage, FileSystemStorage>();
			services.AddTransient<IProjectParser, JsonProjectParser>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
				{
					HotModuleReplacement = true
				});
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");

				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Home", action = "Index" });
			});
		}
	}
}
