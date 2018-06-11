using Microsoft.AspNetCore.Mvc;

namespace VamBooru.Controllers
{
	[Route("/api")]
	public class ServerUtilitiesController : Controller
	{
		[HttpGet("ping")]
		public string Ping()
		{
			return "Ok";
		}
	}
}
