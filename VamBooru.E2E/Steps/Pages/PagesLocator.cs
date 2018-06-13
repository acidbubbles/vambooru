using System;
using OpenQA.Selenium;

namespace VamBooru.E2E.Steps.Pages
{
	public class PagesLocator
	{
		private readonly IWebDriver _browser;
		private readonly Uri _baseUrl;

		public PagesLocator(IWebDriver browser, Uri baseUrl)
		{
			_browser = browser;
			_baseUrl = baseUrl;
		}

		public HomePage Home => new HomePage(_browser, _baseUrl);
		public BrowsePage Browse => new BrowsePage(_browser, _baseUrl);

		public string Title()
		{
			return _browser.Title;
		}

		public string Source()
		{
			return _browser.PageSource;
		}
	}
}
