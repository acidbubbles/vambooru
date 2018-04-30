using VamBooru.Models;

namespace VamBooru.Repository.EFPostgres
{
	public abstract class EntityFrameworkRepositoryBase
	{
		protected VamBooruDbContext DbContext { get; }

		protected EntityFrameworkRepositoryBase(VamBooruDbContext dbContext)
		{
			DbContext = dbContext;
		}
	}
}
