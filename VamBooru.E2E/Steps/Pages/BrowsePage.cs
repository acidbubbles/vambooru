using System;
using System.Linq;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace VamBooru.E2E.Steps.Pages
{
	public class BrowsePage : PageBase
	{
		public BrowsePage(IWebDriver driver, Uri baseUrl) : base(driver, baseUrl)
		{
		}

		public Task Go()
		{
			return GoAndWaitForAngular("/browse");
		}

		public Task Text(string text)
		{
			Browser.FindElement(By.Id("text")).SendKeys(text);
			return Task.CompletedTask;
		}

		public Task Search()
		{
			Browser.FindElement(By.ClassName("btn-primary")).Click();
			return WaitForAngular();
		}

		public Task<PostCard[]> Results()
		{
			return Task.FromResult(Browser.FindElements(By.ClassName("card")).Select(card => new PostCard(card)).ToArray());
		}
	}
}
