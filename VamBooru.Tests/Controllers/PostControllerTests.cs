using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Tests.Controllers
{
	public class PostControllerTests
	{
		[Test]
		public async Task Browse_NoFilters()
		{
			var repository = new Mock<IRepository>(MockBehavior.Strict);
			repository
				.Setup(mock => mock.BrowsePostsAsync(PostSortBy.Default, PostedSince.Default, 0, 10))
				.ReturnsAsync(new[]
				{
					new Post
					{
						Id = Guid.Parse("9495ee61-37ac-43cf-8ee0-dbcd18510914"),
						Title = "My Scene",
						Author = new User {Username = "john.doe"},
						Tags = new[]
						{
							new PostTag {Tag = new Tag {Id = Guid.Parse("c2fa02b7-b107-4261-8306-9465178f2949"), Name = "artsy"}}
						}
					}
				});
			var controller = new PostsController(repository.Object);

			var result = await controller.BrowseAsync();

			result.ShouldDeepEqual(new[]
			{
				new PostViewModel
				{
					Id ="9495ee61-37ac-43cf-8ee0-dbcd18510914",
					Title = "My Scene",
					Author = new UserViewModel {Username = "john.doe"},
					Tags = new[] {new TagViewModel {Id = "c2fa02b7-b107-4261-8306-9465178f2949", Name = "artsy"}}
				}
			});
		}
	}
}
