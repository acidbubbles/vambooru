using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace VamBooru.E2E.WebApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing#integration-testing
				.UseContentRoot(Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "VamBooru")))
				.UseStartup<Startup>();
	}
}
