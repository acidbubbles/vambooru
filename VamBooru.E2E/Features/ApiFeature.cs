using System.Net;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.NUnit3;

namespace VamBooru.E2E.Features
{
	public partial class ApiFeature
	{
		[Scenario]
		public Task server_can_be_pinged()
		{
			return Runner.RunScenarioAsync(
				when => http_get("/api/ping"),
				then => status_code_is(HttpStatusCode.OK)
			);
		}
	}
}
