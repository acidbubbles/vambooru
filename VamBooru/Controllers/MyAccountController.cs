using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/account")]
	public class MyAccountController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly IPostsRepository _postsRepository;

		public MyAccountController(IUsersRepository usersRepository, IPostsRepository postsRepository)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
		}

		[HttpGet("")]
		public async Task<IActionResult> Get()
		{
			if (!User.Identity.IsAuthenticated) return Unauthorized();
			var login = this.GetUserLoginInfo();
			var account = await _usersRepository.LoadPrivateUserAsync(login);

			if (account == null) return NotFound();
			var posts = await _postsRepository.BrowseMyPostsAsync(login);

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
				var dbAccount = await _usersRepository.UpdateUserAsync(this.GetUserLoginInfo(), account.Username);
				return Ok(new MyAccountViewModel { Username = dbAccount.Username });
			}
			catch (UsernameConflictException)
			{
				return StatusCode(409, new { code = "UsernameConflict" });
			}
		}
	}
}
