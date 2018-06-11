using System.Net;
using System.Threading.Tasks;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.NUnit3;

namespace VamBooru.E2E.Features
{
	public partial class BasicWebsiteFeature
	{
		[Scenario]
		public Task home_page_can_be_loaded()
		{
			return Runner.RunScenarioAsync(
				when => http_get("/"),
				then => status_code_is(HttpStatusCode.OK),
				and => content_type_is("text/html")
			);
		}
	}
}
