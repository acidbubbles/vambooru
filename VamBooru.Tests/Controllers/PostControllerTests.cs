using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;
using VamBooru.Models;
using VamBooru.Services;
using VamBooru.Tests.TestUtils;

namespace VamBooru.Tests.Controllers
{
	public class PostControllerTests
	{
		[Test]
		public async Task Browse_DefaultOrder()
		{
			var repository = new Mock<IRepository>(MockBehavior.Strict);
			var storage = new Mock<IStorage>(MockBehavior.Strict);

			var controller = new PostController(repository.Object);

			var result = await controller.BrowseAsync();

			CollectionAssert.AreEqual(new[]
			{
				new Post
				{
					Title = "My super post"
				}
			}, result, new PostViewModelComparer());
		}
	}
}
