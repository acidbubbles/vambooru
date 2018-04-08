using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;
using VamBooru.Models;
using VamBooru.Services;
using VamBooru.Tests.TestUtils;

namespace VamBooru.Tests.Controllers
{
	public class ScenesControllerTests
	{
		[Test]
		public async Task Browse_DefaultOrder()
		{
			var repository = new Mock<IRepository>(MockBehavior.Strict);

			var controller = new ScenesController(repository.Object);

			var result = await controller.Browse();

			CollectionAssert.AreEqual(new[]
			{
				new Scene
				{
					Title = "My super scene",
					ImageUrl = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"
				}
			}, result, new ScenesComparer());
		}
	}
}
