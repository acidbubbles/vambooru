using System;
using Microsoft.Extensions.DependencyInjection;

namespace VamBooru.Storage.EFPostgres
{
	public static class EFPostgresStorageServicesExtensions
	{
		public static IServiceCollection AddEFPostgresStorage(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			services.AddTransient<IStorage, EntityFrameworkStorage>();
			return services;
		}
	}
}
