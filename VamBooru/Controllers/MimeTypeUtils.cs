namespace VamBooru.Controllers
{
	public class MimeTypeUtils
	{
		public static readonly string[] Known = {
			".wav",
			".mp3",
			".jpg",
			".png",
			".txt"
		};

		public static string Of(string extension)
		{
			switch (extension)
			{
				case ".json":
					return "application/json";
				case ".jpg":
					return "image/jpeg";
				case ".wav":
					return "audio/wav";
				case ".mp3":
					return "audio/mpeg";
				case ".txt":
					return "text/plain";
				default:
					return "application/octet-stream";
			}
		}

	}
}
