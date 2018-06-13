using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
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
			var id = GetIdFromUrn(urn);

			var file = await _context.StorageFiles.FindAsync(id);
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

		public Task DeleteFileAsync(string urn)
		{
			var file = new StorageFile {Id = GetIdFromUrn(urn)};
			_context.StorageFiles.Attach(file);
			_context.StorageFiles.Remove(file);
			return _context.SaveChangesAsync();
		}

		private static long GetIdFromUrn(string urn)
		{
			if (!urn.StartsWith(UrnPrefix)) throw new ArgumentException($"Invalid or unsupported URN: '{urn}'", nameof(urn));
			var id = long.Parse(urn.Substring(UrnPrefix.Length));
			return id;
		}
	}
}

