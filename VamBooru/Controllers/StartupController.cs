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
		private readonly IUsersRepository _usersRepository;
		private readonly string _authScheme;

		public StartupController(IConfiguration configuration, IUsersRepository usersRepository)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
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

			var user = await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo());
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
