using OpenQA.Selenium;

namespace VamBooru.E2E.Steps.Pages
{
	public class PostCard
	{
		private readonly IWebElement _card;

		public PostCard(IWebElement card)
		{
			_card = card;
		}

		public string Title() => _card.FindElement(By.ClassName("card-title")).Text.Trim();
	}
}