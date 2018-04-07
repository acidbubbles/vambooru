using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VamBooru.Controllers
{
	[Route("api/[controller]")]
	public class ScenesController : Controller
	{
		[HttpGet("[action]")]
		public Task<Scene[]> Browse()
		{
			return Task.FromResult(new[]
			{
				new Scene
				{
					Title = "My super scene",
					ImageUrl = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"
				}
			});
		}

		public class Scene
		{
			public string Title { get; set; }
			public string ImageUrl { get; set; }
		}
	}
}
