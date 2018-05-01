using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkPostFilesRepository : EntityFrameworkRepositoryBase, IPostFilesRepository
	{
		public EntityFrameworkPostFilesRepository(VamBooruDbContext dbContext)
			: base(dbContext)
		{
		}

		public Task<PostFile[]> LoadPostFilesAsync(Guid postId)
		{
			return DbContext.PostFiles
				.Where(sf => sf.Post.Id == postId)
				.ToArrayAsync();
		}

		public Task<PostFile> LoadPostFileAsync(Guid postId, string urn)
		{
			return DbContext.PostFiles
				.SingleOrDefaultAsync(sf => sf.Urn == urn && sf.Post.Id == postId);
		}
	}
}
