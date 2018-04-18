using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VamBooru.Models;

namespace VamBooru.Tests
{
	public class TestsDbContextFactory : IDesignTimeDbContextFactory<VamBooruDbContext>
	{
		public VamBooruDbContext CreateDbContext(string[] args)
		{
			var currentDirectory = Directory.GetCurrentDirectory();
			// The directory will be different when run in unit tests or using `dotnet ef database update`
			if (Regex.IsMatch(currentDirectory, @"bin[/\\](Debug|Release)"))
				currentDirectory = Path.Combine(currentDirectory, "../../../");
			var configuration = new ConfigurationBuilder()
				.SetBasePath(currentDirectory)
				.AddJsonFile("appsettings.json")
				.AddUserSecrets<TestsDbContextFactory>()
				.Build();

			var connectionString = configuration["Repository:EFPostgres:ConnectionString"] ?? throw new NullReferenceException("The VamBooru connection string was not configured in appsettings.json");

			var builder = new DbContextOptionsBuilder<VamBooruDbContext>();
			builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("VamBooru"));
			return new VamBooruDbContext(builder.Options);
		}
	}
}
