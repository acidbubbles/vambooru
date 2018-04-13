using Microsoft.EntityFrameworkCore;

namespace VamBooru.Models
{
	public class VamBooruDbContext : DbContext
	{
		public VamBooruDbContext(DbContextOptions<VamBooruDbContext> options)
			: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<UserLogin> UserLogins { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<PostTag> PostTags { get; set; }
		public DbSet<Scene> Scenes { get; set; }
		public DbSet<SceneFile> SceneFiles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserLogin>()
				.HasKey(l => new {l.Scheme, Username = l.NameIdentifier});

			modelBuilder.Entity<PostTag>()
				.HasKey(t => new { SceneId = t.PostId, t.TagId });

			modelBuilder.Entity<Tag>()
				.HasIndex(tag => tag.Name)
				.IsUnique();
		}
	}
}
