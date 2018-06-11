using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace VamBooru.E2E.Web
{
	public class TestWebHostBuilderFactory
	{
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing#integration-testing
				.UseContentRoot(Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", "..", "..", "VamBooru")))
				.UseStartup<TestStartup>();
	}
}
