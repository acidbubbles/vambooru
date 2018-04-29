using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Storage.EFPostgres;

namespace VamBooru.Tests.Storage.EFPostgres
{
	public class EntityFrameworkStorageTests
	{
		private VamBooruDbContext _context;
		private EntityFrameworkStorage _storage;

		[SetUp]
		public async Task BeforeEach()
		{
			CreateDbContext();

			await _context.Database.ExecuteSqlCommandAsync("DELETE FROM \"StorageFiles\"");
		}

		[TearDown]
		public void AfterEach()
		{
			_context?.Dispose();
		}

		[Test]
		public async Task CanSaveAndLoadFile_NotCompressed()
		{
			var bytes = new byte[] {1, 2, 3, 4};
			var urn = await _storage.SaveFileAsync(new MemoryStream(bytes), false);
			var memoryStream = new MemoryStream();
			using (var resultStream = await _storage.LoadFileStreamAsync(urn, false))
			{
				await resultStream.CopyToAsync(memoryStream);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			Assert.That(memoryStream.ToArray(), Is.EqualTo(bytes));
		}

		[Test]
		public async Task CanSaveAndLoadFile_Compressed()
		{
			var bytes = new byte[] {1, 2, 3, 4};
			var urn = await _storage.SaveFileAsync(new MemoryStream(bytes), true);
			var memoryStream = new MemoryStream();
			using (var resultStream = await _storage.LoadFileStreamAsync(urn, true))
			{
				await resultStream.CopyToAsync(memoryStream);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			Assert.That(memoryStream.ToArray(), Is.EqualTo(bytes));
		}

		private void CreateDbContext()
		{
			_context?.Dispose();
			_context = new TestsDbContextFactory().CreateDbContext(new string[0]);
			_storage = new EntityFrameworkStorage(_context);
		}
	}
}
