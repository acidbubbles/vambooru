using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("/api/startup")]
	public class StartupController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly ILogger<StartupController> _logger;
		private readonly string _authScheme;

		public StartupController(IConfiguration configuration, IUsersRepository usersRepository, ILogger<StartupController> logger)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

			var loginInfo = this.GetUserLoginInfo();
			var user = await _usersRepository.LoadPrivateUserAsync(loginInfo);

			if (user == null)
			{
				_logger.LogWarning($"User '{loginInfo.NameIdentifier}' in scheme '{loginInfo.Scheme}' was logged in, but user does not exist in database.");
				await HttpContext.SignOutAsync();
				return Ok(new StartupConfiguration
				{
					IsAuthenticated = false,
					AuthSchemes = new[] {_authScheme}
				});
			}

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
