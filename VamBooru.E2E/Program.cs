using Microsoft.AspNetCore.Hosting;
using VamBooru.E2E.Web;

namespace VamBooru.E2E
{
	public class Program
	{
		public static void Main(string[] args)
		{
			TestWebHostBuilderFactory.CreateWebHostBuilder(args).Build().Run();
		}
	}
}
