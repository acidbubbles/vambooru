using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VamBooru.Services
{
	public class JsonProjectParser : IProjectParser
	{
		public string[] GetTagsFromProject(byte[] projectStream)
		{
			var project = ParseProjectJson(projectStream);

			if (project == null) throw new NullReferenceException("Project was null after JSON parsing");

			var tags = ((IEnumerable<string>)ExtractTagsFromProject(project)).ToArray();

			return tags;
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
				yield return $"{males}-male{(males > 1 ? "s" : "")}";

			if (females > 0)
				yield return $"{females}-female{(males > 1 ? "s" : "")}";
		}
	}
}
