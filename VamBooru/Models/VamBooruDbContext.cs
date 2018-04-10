using Microsoft.EntityFrameworkCore;

namespace VamBooru.Models
{
	public class VamBooruDbContext : DbContext
	{
		public VamBooruDbContext(DbContextOptions<VamBooruDbContext> options)
			: base(options)
		{
		}

		public DbSet<Author> Authors { get; set; }
		public DbSet<Scene> Scenes { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<SceneTag> SceneTags { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<SceneTag>()
				.HasKey(t => new { t.SceneId, t.TagId });

			modelBuilder.Entity<Tag>()
				.HasIndex(tag => tag.Name)
				.IsUnique();
		}
	}
}
