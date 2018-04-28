using System;
using System.IO;
using System.Text.RegularExpressions;

namespace VamBooru.Controllers
{
	public static class SanitizingUtils
	{
		private static readonly Regex InvalidFilenameCharactersRegex = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled, TimeSpan.FromSeconds(1));

		public static string GetSanitizedFilename(string filename)
		{
			var result = InvalidFilenameCharactersRegex.Replace(filename, "");
			if (string.IsNullOrWhiteSpace(result)) throw new Exception("Invalid username");
			return result;
		}
	}
}