using Microsoft.Extensions.Configuration;

namespace VamBooru.E2E.WebApp
{
	public class Startup : VamBooru.Startup
	{
		public Startup(IConfiguration configuration) : base(configuration)
		{
		}
	}
}
