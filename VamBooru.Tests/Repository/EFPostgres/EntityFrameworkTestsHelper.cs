using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VamBooru.Models;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public static class EntityFrameworkTestsHelper
	{
		private static readonly string[] Tables = {
			"PostTags",
			"Tags",
			"UserPostVotes",
			"PostComments",
			"PostFiles",
			"Scenes",
			"Posts",
			"UserLogins",
			"Users",
			// When database blob storage is used
			"StorageFiles"
		};

		public static async Task ClearAndMarkTestDatabase(VamBooruDbContext dbContext)
		{
			if (dbContext.Database.GetDbConnection().Database.EndsWith("Test"))
				throw new UnauthorizedAccessException("Cannot use a non-test database in tests");

			var tables = dbContext.Model.GetEntityTypes().Select(entity => entity.Relational().TableName).ToArray();

			if(!new HashSet<string>(Tables).SetEquals(tables))
				throw new InvalidOperationException($"Tables to delete do not match tables specified in DbContext entities: {string.Join(", ", tables)}"); 

			foreach (var table in Tables)
			{
				try
				{
					var sql = $"DELETE FROM \"{table}\"";
					#pragma warning disable EF1000
					await dbContext.Database.ExecuteSqlCommandAsync(sql);
					#pragma warning restore EF1000
				}
				catch (PostgresException exc)
				{
					throw new InvalidOperationException($"Could not delete table {table} - check that the delete sequence respects constraints.", exc);
				}
			}
		}
	}
}
