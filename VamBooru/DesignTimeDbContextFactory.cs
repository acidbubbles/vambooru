using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VamBooru.Models;

namespace VamBooru
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VamBooruDbContext>
	{
		public VamBooruDbContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<VamBooruDbContext>();
			builder.UseNpgsql(configuration.GetConnectionString("VamBooru") ?? throw new NullReferenceException("The VamBooru connection string was not configured in appsettings.json"));
			return new VamBooruDbContext(builder.Options);
		}
	}
}
