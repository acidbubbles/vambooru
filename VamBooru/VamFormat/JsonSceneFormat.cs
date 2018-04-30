using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VamBooru.VamFormat
{
	public class JsonSceneFormat : ISceneFormat
	{
		public object Deserialize(byte[] jsonFile)
		{
			var serializer = new JsonSerializer();
			using (var jsonStream = new MemoryStream(jsonFile))
			using (var streamReader = new StreamReader(jsonStream))
			using (var jsonReader = new JsonTextReader(streamReader))
				return serializer.Deserialize(jsonReader);
		}

		public string[] GetTags(dynamic project)
		{
			if (project == null) throw new NullReferenceException("Project was null after JSON parsing");

			var tags = ((IEnumerable<string>)ExtractTagsFromProject(project)).ToArray();

			return tags;
		}

		private static IEnumerable<string> ExtractTagsFromProject(dynamic project)
		{
			if (!(project.atoms is JArray atoms))
			{
				yield break;
			}

			var males = 0;
			var females = 0;
			var audio = 0;
			var triggers = 0;
			foreach (dynamic atom in atoms)
			{
				var storables = atom.storables;
				if (storables == null) continue;

				foreach (var storable in storables)
				{
					if (storable.triggers != null && storable.triggers.Count > 0)
						triggers++;

					if (storable.id == "geometry")
					{
						var character = storable.character?.ToString();
						if (character == null) continue;
						if (character.StartsWith("Female")) females++;
						if (character.StartsWith("Male")) males++;
					}
					else if (storable.id == "URLAudioClipManager")
					{
						if (storable.clips != null && storable.clips.Count > 0) audio++;
					}
				}
			}

			if (males > 0)
				yield return $"{males}-male{(males > 1 ? "s" : "")}";

			if (females > 0)
				yield return $"{females}-female{(females > 1 ? "s" : "")}";

			if (audio > 0)
				yield return "audio";

			if (triggers > 0)
				yield return "interactive";
		}
	}
}
