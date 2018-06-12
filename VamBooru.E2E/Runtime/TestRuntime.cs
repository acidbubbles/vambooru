using System;
using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using VamBooru.E2E.Steps.Pages;

namespace VamBooru.E2E.Runtime
{
	public class TestRuntime
	{
		public static Uri ServerBaseUrl { get; private set; }
		public static HttpClient HttpClient { get; private set; }
		public static IWebDriver Browser { get; private set; }
		public static PagesLocator Pages { get; private set; }

		public static void InitializeServer(Uri uri)
		{
			if (ServerBaseUrl != null) throw new InvalidOperationException("Server already registered");

			ServerBaseUrl = uri;
			HttpClient = new HttpClient
			{
				BaseAddress = ServerBaseUrl
			};
		}

		public static void InitializeDriver(ChromeDriver driver)
		{
			if(ServerBaseUrl == null) throw new InvalidOperationException("Server must be initialized first");
			if (Browser != null) throw new InvalidOperationException("Browser already registered");

			Browser = driver;
			Pages = new PagesLocator(driver, ServerBaseUrl);
		}
	}
}
