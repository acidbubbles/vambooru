using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace VamBooru.E2E.Steps.Pages
{
	public class PageBase
	{
		private static readonly TimeSpan MaxWaitTime = new TimeSpan(0, 0, 30);

		public IWebDriver Browser { get; set; }
		public Uri BaseUrl { get; }

		protected PageBase(IWebDriver browser, Uri baseUrl)
		{
			Browser = browser;
			BaseUrl = baseUrl;
		}

		protected Task GoAndWaitForAngular(string url)
		{
			Browser.Navigate().GoToUrl(new Uri(BaseUrl, url));
			return WaitForAngular();
		}

		protected Task WaitForAngular()
		{
			var wait = new WebDriverWait(Browser, MaxWaitTime);
			var success = wait.Until(CheckForAngularReady);
			if (!success) throw new TimeoutException("Timeout while waiting for angular to be ready again.");
			return Task.CompletedTask;
		}

		private static bool CheckForAngularReady(IWebDriver browser)
		{
			if (browser == null) throw new ArgumentNullException(nameof(browser));
			const string angularReadyFn = "return document.readyState === 'complete' && window.getAllAngularTestabilities && window.getAllAngularTestabilities().findIndex(x=>!x.isStable()) === -1";
			var scriptExecutor = (IJavaScriptExecutor)browser;
			var result = scriptExecutor.ExecuteScript(angularReadyFn);
			return result is bool isReady && isReady;
		}
	}
}
