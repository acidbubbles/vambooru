using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;
using VamBooru.Repository;

namespace VamBooru.Controllers
{
	[Route("/api/admin")]
	public class AdminController : Controller
	{
		private readonly IUsersRepository _usersRepository;
		private readonly VamBooruDbContext _dbContext;

		public AdminController(IUsersRepository usersRepository, VamBooruDbContext dbContext)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		[HttpGet("fix-double-compressed")]
		public async Task<IActionResult> FixDoubleCompressed()
		{
			var result = new List<string>();
			foreach (var file in await _dbContext.PostFiles.ToArrayAsync())
			{
				if (!file.Compressed) continue;
				var storageId = int.Parse(file.Urn.Substring("urn:vambooru:ef:".Length));
				var storage = await _dbContext.StorageFiles.SingleAsync(sf => sf.Id == storageId);
				var data = storage.Bytes;
				while (true)
				{
					try
					{
						data = Decompress(data);
					}
					catch (InvalidDataException)
					{
						break;
					}
				}

				storage.Bytes = Compress(data);
				await _dbContext.SaveChangesAsync();
				result.Add(file.Urn);
			}

			return Ok(result);
		}

		public byte[] Compress(byte[] data)
		{
			var result = new MemoryStream();
			using (var gzip = new GZipStream(result, CompressionLevel.Optimal, true))
			{
				gzip.Write(data, 0, data.Length);
			}
			result.Seek(0, SeekOrigin.Begin);
			return result.ToArray();
		}

		public byte[] Decompress(byte[] data)
		{
			var original = new MemoryStream(data);
			var decompressed = new MemoryStream();
			using (var gzip = new GZipStream(original, CompressionMode.Decompress, true))
			{
				gzip.CopyTo(decompressed);
			}
			decompressed.Seek(0, SeekOrigin.Begin);
			return decompressed.ToArray();
		}

		[HttpGet("stats")]
		public async Task<IActionResult> GetStats()
		{
			if ((await _usersRepository.LoadPrivateUserAsync(this.GetUserLoginInfo())).Role != UserRoles.Admin)
				return Unauthorized();

			var users = await _dbContext.Users.CountAsync();
			var posts = await _dbContext.Posts.CountAsync();
			var tags = await _dbContext.Tags.CountAsync();

			return Ok(new
			{
				users,
				posts,
				tags
			});
		}
	}
}
