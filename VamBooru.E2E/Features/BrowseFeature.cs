using System.Net;
using System.Threading.Tasks;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.NUnit3;

namespace VamBooru.E2E.Features
{
	public partial class BrowseFeature
	{
		[Scenario]
		public Task can_browse_by_text()
		{
			return Runner.RunScenarioAsync(
				when => navigating_to_browse_page(),
				and => entering_search_text("Jumping"),
				and => selecting_browse(),
				then => search_results_are("Jumping Lady")
			);
		}
	}
}
