using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VamBooru.Controllers
{
	[Route("api/[controller]")]
	public class ScenesController : Controller
	{
		private readonly string _outputFolder;

		public ScenesController(IConfiguration configuration)
		{
			_outputFolder = configuration["VamBooru:ProjectsPath"];
		}

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

			var projectStream = await CopyToMemoryStream(jsonFile);
			var project = ParseProjectJson(projectStream.ToArray());

			if (project == null) return BadRequest(new UploadResponse { Success = false, Code = "ProjectNull" });

			var tags = ((IEnumerable<string>)ExtractTagsFromProject(project)).ToArray();

			string id;
			using(var imageStream = imageFile.OpenReadStream())
				id = await SaveProject(projectStream, imageStream, tags);

			return Ok(new UploadResponse { Success = true, Id = id });
		}

		private async Task<string> SaveProject(Stream projectStream, Stream imageStream, string[] tags)
		{
			var guid = Guid.NewGuid().ToString();

			using (var jsonWriter = System.IO.File.OpenWrite(Path.Combine(_outputFolder, $"{guid}.json")))
				await projectStream.CopyToAsync(jsonWriter);

			using (var thumbnailWriter = System.IO.File.OpenWrite(Path.Combine(_outputFolder, $"{guid}.jpg")))
				await imageStream.CopyToAsync(thumbnailWriter);

			return guid;
		}

		private static async Task<MemoryStream> CopyToMemoryStream(IFormFile file)
		{
			var projectBytes = new MemoryStream();
			using (var stream = file.OpenReadStream())
			{
				await stream.CopyToAsync(projectBytes);
			}

			projectBytes.Seek(0, SeekOrigin.Begin);
			return projectBytes;
		}

		private static dynamic ParseProjectJson(byte[] jsonFile)
		{
			var serializer = new JsonSerializer();
			using (var jsonStream = new MemoryStream(jsonFile))
			using (var streamReader = new StreamReader(jsonStream))
			using (var jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize(jsonReader);
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
			public string Id { get; set; }
		}
	}
}
