using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Storage.EFPostgres
{
	public class EntityFrameworkStorage : IStorage
	{
		private const string UrnPrefix = "urn:vambooru:ef:";

		private readonly VamBooruDbContext _context;

		public EntityFrameworkStorage(VamBooruDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<string> SaveFileAsync(MemoryStream stream, bool compressed)
		{
			if (compressed)
			{
				var result = new MemoryStream();
				using (var gzip = new GZipStream(result, CompressionLevel.Optimal, true))
				{
					stream.CopyTo(gzip);
				}
				result.Seek(0, SeekOrigin.Begin);
				stream = result;
			}

			var file = new StorageFile
			{
				Bytes = stream.ToArray()
			};
			_context.StorageFiles.Add(file);
			await _context.SaveChangesAsync();
			return $"{UrnPrefix}{file.Id}";
		}

		public async Task<Stream> LoadFileStreamAsync(string urn, bool compressed)
		{
			if(!urn.StartsWith("urn:vambooru:ef:")) throw new ArgumentException($"Invalid or unsupported URN: '{urn}'", nameof(urn));
			var id = int.Parse(urn.Substring(UrnPrefix.Length));

			var file = await _context.StorageFiles.FirstOrDefaultAsync(sf => sf.Id == id);
			if (file == null) return null;

			if (compressed)
			{
				var original = new MemoryStream(file.Bytes);
				var decompressed = new MemoryStream();
				using (var gzip = new GZipStream(original, CompressionMode.Decompress, true))
				{
					gzip.CopyTo(decompressed);
				}
				decompressed.Seek(0, SeekOrigin.Begin);
				return decompressed;
			}

			return new MemoryStream(file.Bytes);
		}
	}
}

