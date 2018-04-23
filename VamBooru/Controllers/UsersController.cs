using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/users")]
	public class UsersController : Controller
	{
		private const string Me = "me";

		private readonly IRepository _repository;

		public UsersController(IRepository repository)
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
				user = await _repository.LoadPublicUserAsync(Guid.Parse(userId));
			}

			if (user == null) return NotFound();

			return Ok(UserViewModel.From(user));
		}

		[HttpPut("{userId}")]
		public async Task<IActionResult> Put([FromRoute] string userId, [FromBody] UserViewModel user)
		{
			if (userId != Me || !User.Identity.IsAuthenticated) return Unauthorized();
			//TODO: Handle username conflicts
			var dbUser = await _repository.UpdateUserAsync(this.GetUserLoginInfo(), user);
			return Ok(UserViewModel.From(dbUser));
		}
	}
}

