using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using VamBooru.Services;

namespace VamBooru.Tests.Services
{
	public class JsonSceneParserTests
	{
		public class Tags
		{
			private JsonSceneParser _parser;

			[SetUp]
			public void BeforeEach()
			{
				_parser = new JsonSceneParser();
			}

			[Test]
			public void HandlesInvalidProjects()
			{
				Assert.IsEmpty(_parser.GetTags(BytesOf(new
				{
					atoms = new[] { new { storables = new[] { new {} } } }
				})));

				Assert.IsEmpty(_parser.GetTags(BytesOf(new
				{
					atoms = new[] { new {} }
				})));

				Assert.IsEmpty(_parser.GetTags(BytesOf(new
				{
				})));
			}

			[Test]
			public void CanFindNothing()
			{
				var tags = _parser.GetTags(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry" } } }
					}
				}));

				Assert.That(tags, Is.Empty);
			}

			[Test]
			public void CanFindOneMaleTwoFemales()
			{
				var tags = _parser.GetTags(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 2" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 10" } } },
					}
				}));

				Assert.That(tags, Is.EquivalentTo(new[] {"1-male", "2-females"}));
			}

			[Test]
			public void CanFindOneFemaleTwoMales()
			{
				var tags = _parser.GetTags(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 3" } } },
					}
				}));

				Assert.That(tags, Is.EquivalentTo(new[] {"2-males", "1-female"}));
			}

			private static byte[] BytesOf(object json)
			{
				var str = JsonConvert.SerializeObject(json, Formatting.Indented);
				return Encoding.UTF8.GetBytes(str);
			}
		}
	}
}
