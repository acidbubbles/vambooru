using System;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace VamBooru.E2E.Steps.Pages
{
	public class HomePage : PageBase
	{
		public HomePage(IWebDriver driver, Uri baseUrl) : base(driver, baseUrl)
		{
		}

		public Task Go()
		{
			return GoAndWaitForAngular("/browse");
		}
	}
}
