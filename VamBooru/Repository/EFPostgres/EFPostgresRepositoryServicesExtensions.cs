using System;
using Microsoft.Extensions.DependencyInjection;

namespace VamBooru.Repository.EFPostgres
{
	public static class EFPostgresStorageServicesExtensions
	{
		public static IServiceCollection AddEFPostgresRepository(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			services.AddTransient<IRepository, EntityFrameworkRepository>();
			return services;
		}
	}
}
