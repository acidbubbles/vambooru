using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using VamBooru.VamFormat;

namespace VamBooru.Tests.VamFormat
{
	public class JsonSceneFormatTests
	{
		public class Tags
		{
			private JsonSceneFormat _format;

			[SetUp]
			public void BeforeEach()
			{
				_format = new JsonSceneFormat();
			}

			[Test]
			public void HandlesInvalidProjects()
			{
				Assert.IsEmpty(_format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[] { new { storables = new[] { new {} } } }
				}))));

				Assert.IsEmpty(_format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[] { new {} }
				}))));

				Assert.IsEmpty(_format.GetTags(Deserialize(BytesOf(new
				{
				}))));
			}

			private dynamic Deserialize(byte[] bytes)
			{
				return _format.Deserialize(bytes);
			}

			[Test]
			public void CanFindNothing()
			{
				var tags = _format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry" } } }
					}
				})));

				Assert.That(tags, Is.Empty);
			}

			[Test]
			public void CanFindAudioClips()
			{
				var tags = _format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[]
					{
						new { id = "CoreControl", storables = new[] { new { id = "URLAudioClipManager", clips = new[] { new { url = "sound.wav" } } } } }
					}
				})));

				Assert.That(tags, Is.EquivalentTo(new[] {"audio"}));
			}

			[Test]
			public void CanFindTriggers()
			{
				var tags = _format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[]
					{
						new { id = "AnimationPattern#1", storables = new[] { new { id = "AnimationPattern", triggers = new[] { new { displayName = "" } } } } }
					}
				})));

				Assert.That(tags, Is.EquivalentTo(new[] {"interactive"}));
			}

			[Test]
			public void CanFindOneMaleTwoFemales()
			{
				var tags = _format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 2" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 10" } } },
					}
				})));

				Assert.That(tags, Is.EquivalentTo(new[] {"1-male", "2-females"}));
			}

			[Test]
			public void CanFindOneFemaleTwoMales()
			{
				var tags = _format.GetTags(Deserialize(BytesOf(new
				{
					atoms = new[]
					{
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Male 1" } } },
						new { storables = new[] { new { id = "geometry", character = "Female 3" } } },
					}
				})));

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
