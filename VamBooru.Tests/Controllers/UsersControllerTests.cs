using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Controllers
{
	public class UsersControllerTests
	{
		[Test]
		public async Task Get_ByUsername()
		{
			var repository = new Mock<IRepository>(MockBehavior.Strict);
			repository
				.Setup(mock => mock.LoadPublicUserAsync("johnny"))
				.ReturnsAsync(new User
				{
					Username = "johnny"
				});
			var controller = new UsersController(repository.Object);

			var result = await controller.Get("johnny");
			var model = ((OkObjectResult)result).Value as UserViewModel;

			model.ShouldDeepEqual(new UserViewModel
			{
				Username = "johnny"
			});
		}
	}
}
