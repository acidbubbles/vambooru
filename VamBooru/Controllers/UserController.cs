using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/users")]
	public class UserController : Controller
	{
		private const string Me = "me";

		private readonly IRepository _repository;

		public UserController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("{userId}")]
		public async Task<IActionResult> Get([FromRoute] string userId)
		{
			User user;
			if (userId == Me)
			{
				if (!User.Identity.IsAuthenticated) return Unauthorized();

				user = await _repository.LoadPrivateUserAsync(User.Identity.AuthenticationType, User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
			}
			else
			{
				user = await _repository.LoadPublicUserAsync(userId);
			}

			if (user == null) return NotFound();

			return Ok(user.ToViewModel());
		}

		[HttpPut("{userId}")]
		public async Task<IActionResult> Put([FromRoute] string userId, [FromBody] UserViewModel user)
		{
			if (userId != Me || !User.Identity.IsAuthenticated) return Unauthorized();
			await _repository.UpdateUserAsync(this.GetUserLoginInfo(), user);
			return NoContent();
		}
	}
}

