using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;

namespace VamBooru.E2E.Controllers
{
	[Route("api/tests")]
	public class TestsController : Controller
	{
		public TestsController(IUsersRepository usersRepository)
		{
		}

		[HttpGet("")]
		public string Root()
		{
			return "Test server running";
		}
	}
}
