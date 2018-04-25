using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/account")]
	public class MyAccountController : Controller
	{
		private readonly IRepository _repository;

		public MyAccountController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("")]
		public async Task<IActionResult> Get()
		{
			if (!User.Identity.IsAuthenticated) return Unauthorized();
			var login = this.GetUserLoginInfo();
			var account = await _repository.LoadPrivateUserAsync(login);

			if (account == null) return NotFound();
			var posts = await _repository.BrowseMyPostsAsync(login);

			return Ok(new MyAccountViewModel
			{
				Username = account.Username,
				MyPosts = posts.Select(p => PostViewModel.From(p, true)).ToArray()
			});
		}

		[HttpPut("")]
		public async Task<IActionResult> Put([FromBody] UpdateMyAccountViewModel account)
		{
			if (!User.Identity.IsAuthenticated) return Unauthorized();
			if (string.IsNullOrWhiteSpace(account.Username)) return BadRequest();

			try
			{
				var dbAccount = await _repository.UpdateUserAsync(this.GetUserLoginInfo(), account.Username);
				return Ok(new MyAccountViewModel { Username = dbAccount.Username });
			}
			catch (UsernameConflictException)
			{
				return StatusCode(409, new { code = "UsernameConflict" });
			}
		}
	}
}
