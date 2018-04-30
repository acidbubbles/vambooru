using System;
using Microsoft.Extensions.DependencyInjection;

namespace VamBooru.Repository.EFPostgres
{
	public static class EFPostgresStorageServicesExtensions
	{
		public static IServiceCollection AddEFPostgresRepository(this IServiceCollection services)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			services.AddTransient<IPostsRepository, EntityFrameworkPostsRepository>();
			services.AddTransient<IPostFilesRepository, EntityFrameworkPostFilesRepository>();
			services.AddTransient<ITagsRepository, EntityFrameworkTagsRepository>();
			services.AddTransient<IUsersRepository, EntityFrameworkUsersRepository>();
			services.AddTransient<IVotesRepository, EntityFrameworkVotesRepository>();
			services.AddTransient<IPostCommentsRepository, EntityFrameworkPostCommentsRepository>();
			return services;
		}
	}
}
