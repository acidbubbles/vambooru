using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("/api/admin")]
	public class AdminController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly VamBooruDbContext _dbContext;

		public AdminController(IUsersRepository usersRepository, VamBooruDbContext dbContext)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		[HttpGet("stats")]
		public async Task<IActionResult> MigrationsExtractStorage()
		{
			if ((await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo())).Role != UserRoles.Admin)
				return Unauthorized();

			var users = await _dbContext.Users.CountAsync();
			var posts = await _dbContext.Posts.CountAsync();
			var tags = await _dbContext.Tags.CountAsync();

			return Ok(new
			{
				users,
				posts,
				tags
			});
		}
	}
}
