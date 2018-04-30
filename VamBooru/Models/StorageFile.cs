using System.ComponentModel.DataAnnotations;

namespace VamBooru.Models
{
	public class StorageFile
	{
		public long Id { get; set; }
		[Required] public byte[] Bytes { get; set; }

		public override string ToString()
		{
			return $"{nameof(StorageFile)} ({Bytes?.Length ?? -1} bytes)";
		}
	}
}
