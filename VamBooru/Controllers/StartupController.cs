using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("/api/startup")]
	public class StartupController : Controller
	{
		private readonly IRepository _repository;
		private readonly string _authScheme;

		public StartupController(IConfiguration configuration, IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_authScheme = configuration["Authentication:Scheme"];
		}

		[HttpGet("")]
		public async Task<IActionResult> Get()
		{
			if (!User.Identity.IsAuthenticated)
				return Ok(new StartupConfiguration
				{
					IsAuthenticated = false,
					AuthSchemes = new[] { _authScheme }
				});

			var user = await _repository.LoadPrivateUserAsync(this.GetUserLoginInfo());
			return Ok(new StartupConfiguration
			{
				IsAuthenticated = true,
				Username = user.Username,
				AuthSchemes = new[] { _authScheme }
			});
		}

		public class StartupConfiguration
		{
			public bool IsAuthenticated { get; set; }
			public string Username { get; set; }
			public string[] AuthSchemes { get; set; }
		}
	}
}
