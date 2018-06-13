using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace VamBooru.E2E.Web
{
	public class TestWebHostBuilderFactory
	{
		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			var projectPath = Path.GetFullPath(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "..", "..", ".."));
			return WebHost.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration(builder =>
				{
					builder.AddJsonFile(Path.Combine(projectPath, "appsettings.json"));
					builder.AddUserSecrets<TestWebHostBuilderFactory>();
				})
				.UseContentRoot(Path.Combine(projectPath, "..", "VamBooru"))
				.UseStartup<TestStartup>();
		}
	}
}
