using System.Collections;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using VamBooru.Controllers;

namespace VamBooru.Tests.Controllers
{
	public class ScenesControllerTests
	{
		[Test]
		public async Task Browse_DefaultOrder()
		{
			var configuration = new Mock<IConfiguration>(MockBehavior.Strict);
			configuration.Setup(mock => mock["VamBooru:ProjectsPath"]).Returns("/projects");
			var controller = new ScenesController(configuration.Object);

			var result = await controller.Browse();

			CollectionAssert.AreEqual(new[]
			{
				new ScenesController.Scene
				{
					Title = "My super scene",
					ImageUrl = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"
				}
			}, result, new ScenesComparer());
		}
	}

	public class ScenesComparer : IComparer
	{
		public int Compare(object o1, object o2)
		{
			if (!(o1 is ScenesController.Scene scene1)) return -1;
			if (!(o2 is ScenesController.Scene scene2)) return -1;
			return scene1.Title == scene2.Title && scene1.ImageUrl == scene2.ImageUrl ? 0 : 1;
		}
	}
}
