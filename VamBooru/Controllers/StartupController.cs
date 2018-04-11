using Microsoft.AspNetCore.Mvc;

namespace VamBooru.Controllers
{
	[Route("/api/startup")]
	public class StartupController : Controller
	{
		[HttpGet("")]
		public IActionResult Get()
		{
			return Ok(new StartupConfiguration
			{
				IsAuthenticated = User.Identity.IsAuthenticated
			});
		}

		public class StartupConfiguration
		{
			public bool IsAuthenticated { get; set; }
		}
	}
}
