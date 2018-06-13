using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.E2E.Steps;

namespace VamBooru.E2E.Features
{
	public partial class BrowseFeature : FeatureFixtureWithCommonSteps
	{
		private Task navigating_to_browse_page()
		{
			return Pages.Browse.Go();
		}

		private Task entering_search_text(string text)
		{
			return Pages.Browse.Text(text);
		}

		private Task selecting_browse()
		{
			return Pages.Browse.Search();
		}

		private async Task search_results_are(params string[] expectedTitles)
		{
			var results = await Pages.Browse.Results();
			var actualTitles = results.Select(r => r.Title()).ToArray();
			Assert.That(results.Length, Is.EqualTo(expectedTitles.Length), string.Join(", ", actualTitles));
			Assert.That(actualTitles, Is.EquivalentTo(expectedTitles));
		}
	}
}
