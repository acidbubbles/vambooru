using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LightBDD.NUnit3;
using NUnit.Framework;

namespace VamBooru.E2E.Steps
{
	public partial class FeatureFixtureWithCommonSteps : FeatureFixture
	{
		private HttpResponseMessage _result;

		#region when

		protected async Task http_get(string path)
		{
			_result = await TestRuntime.HttpClient.GetAsync(path);
		}

		#endregion

		#region then

		protected Task status_code_is(HttpStatusCode statusCode)
		{
			Assert.That(_result?.StatusCode, Is.EqualTo(statusCode));
			return Task.CompletedTask;
		}

		protected Task content_type_is(string contentType)
		{
			Assert.That(_result?.Content?.Headers.ContentType?.MediaType, Is.EqualTo(contentType), () => _result.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
			return Task.CompletedTask;
		}

		#endregion
	}
}
