using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VamBooru.Controllers
{
	public class HomeController : Controller
	{
		[GenerateAntiforgeryTokenCookieForAjax]
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Error()
		{
			ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
			return View();
		}
	}
}
