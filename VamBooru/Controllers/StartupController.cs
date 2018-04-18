using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("/api/startup")]
	public class StartupController : Controller
	{
		private readonly IRepository _repository;

		public StartupController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("")]
		public async Task<IActionResult> Get()
		{
			if (!User.Identity.IsAuthenticated)
				return Ok(new StartupConfiguration
				{
					IsAuthenticated = false
				});

			var user = await _repository.LoadPrivateUserAsync(this.GetUserLoginInfo());
			return Ok(new StartupConfiguration
			{
				IsAuthenticated = true,
				Username = user.Username
			});
		}

		public class StartupConfiguration
		{
			public bool IsAuthenticated { get; set; }
			public string Username { get; set; }
		}
	}
}
