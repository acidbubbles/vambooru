using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Tests.Controllers
{
	public class TagsControllerTests
	{
		[Test]
		public async Task SearchTags()
		{
			var repository = new Mock<ITagsRepository>(MockBehavior.Strict);
			repository
				.Setup(mock => mock.SearchTags("art"))
				.ReturnsAsync(new[]
				{
					new Tag 
					{
						Id = Guid.Parse("c2fa02b7-b107-4261-8306-9465178f2949"),
						Name = "artsy"
					}
				});
			var controller = new TagsController(repository.Object);

			var result = await controller.SearchTags("art");

			result.ShouldDeepEqual(new[]
			{
				new TagViewModel
				{
					Name = "artsy"
				}
			});
		}

		[Test]
		public async Task LoadTopTags()
		{
			var repository = new Mock<ITagsRepository>(MockBehavior.Strict);
			repository
				.Setup(mock => mock.LoadTopTags(16))
				.ReturnsAsync(new[]
				{
					new Tag 
					{
						Id = Guid.Parse("c2fa02b7-b107-4261-8306-9465178f2949"),
						Name = "artsy"
					}
				});
			var controller = new TagsController(repository.Object);

			var result = await controller.LoadTopTags();

			result.ShouldDeepEqual(new[]
			{
				new TagViewModel
				{
					Name = "artsy"
				}
			});
		}
	}
}
