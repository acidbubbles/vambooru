using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VamBooru.Controllers
{
	[Route("api/[controller]")]
	public class ScenesController : Controller
	{
		[HttpGet("[action]")]
		public Task<Scene[]> Browse()
		{
			return Task.FromResult(new[]
			{
				new Scene
				{
					Title = "My super scene",
					ImageUrl = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"
				}
			});
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Upload()
		{
			var files = Request.Form.Files;

			var jsonFile = files.FirstOrDefault(f => f.Name == "json");
			if (jsonFile == null) return BadRequest(new UploadResponse { Success = false, Code = "JsonMissing" });

			var imageFile = files.FirstOrDefault(f => f.Name == "thumbnail");
			if (imageFile == null) return BadRequest(new UploadResponse { Success = false, Code = "ThumbnailMissing" });

			var project = ParseProjectJson(jsonFile);

			if (project == null) return BadRequest(new UploadResponse { Success = false, Code = "ProjectNull" });

			var tags = ExtractTagsFromProject(project).ToArray();

			return Ok(new UploadResponse { Success = true });
		}

		private static IEnumerable<string> ExtractTagsFromProject(dynamic project)
		{
			if (!(project.atoms is JArray atoms))
			{
				yield return "empty";
				yield break;
			}

			var males = 0;
			var females = 0;
			foreach (dynamic atom in atoms)
			{
				var storables = atom.storables;
				if (storables == null) continue;

				foreach (var storable in storables)
				{
					if (storable.id != "geometry") continue;
					var character = storable.character?.ToString();
					if (character == null) continue;
					if (character.StartsWith("Female")) females++;
					if (character.StartsWith("Male")) males++;
				}
			}

			if (males > 0)
				yield return $"{males}-males";

			if (females > 0)
				yield return $"{females}-females";
		}

		private static dynamic ParseProjectJson(IFormFile jsonFile)
		{
			dynamic project;
			var serializer = new JsonSerializer();
			using (var jsonStream = jsonFile.OpenReadStream())
			using (var streamReader = new StreamReader(jsonStream))
			using (var jsonReader = new JsonTextReader(streamReader))
				project = serializer.Deserialize(jsonReader);
			return project;
		}

		public class Scene
		{
			public string Title { get; set; }
			public string ImageUrl { get; set; }
			public string[] Tags { get; set; }
		}

		public class UploadResponse
		{
			public bool Success { get;set; }
			public string Code { get; set; }
		}
	}
}
