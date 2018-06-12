using System;
using OpenQA.Selenium;

namespace VamBooru.E2E.Steps.Pages
{
	public class HomePage : PageBase
	{
		public HomePage(IWebDriver driver, Uri baseUrl) : base(driver, baseUrl)
		{
		}

		public void Go()
		{
			GoAndWaitForAngular("/");
		}
	}
}
