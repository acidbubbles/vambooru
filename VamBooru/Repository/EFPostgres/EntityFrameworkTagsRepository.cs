using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkTagsRepository : EntityFrameworkRepositoryBase, ITagsRepository
	{
		public EntityFrameworkTagsRepository(VamBooruDbContext dbContext)
			: base(dbContext)
		{
		}

		public Task<Tag[]> SearchTags(string q)
		{
			return DbContext.Tags
				.Where(t => t.Name.Contains(q))
				.ToArrayAsync();
		}

		public Task<Tag[]> LoadTopTags(int max)
		{
			return DbContext.Tags
				.Where(t => t.PostsCount > 0)
				.OrderByDescending(t => t.PostsCount)
				.ThenBy(t => t.Name)
				.ToArrayAsync();
		}
	}
}
