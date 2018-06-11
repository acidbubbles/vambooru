using System;
using System.Net.Http;

namespace VamBooru.E2E.Steps
{
	public class TestRuntime
	{
		public static Uri ServerBaseUrl { get; private set; }
		public static HttpClient HttpClient { get; private set; }

		public static void Initialize(Uri uri)
		{
			ServerBaseUrl = uri;
			HttpClient = new HttpClient
			{
				BaseAddress = ServerBaseUrl
			};
		}
	}
}
