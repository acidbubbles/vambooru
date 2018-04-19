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
		public DbSet<SupportFile> SupportFiles { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<PostTag> PostTags { get; set; }
		public DbSet<Scene> Scenes { get; set; }
		public DbSet<SceneFile> SceneFiles { get; set; }
		public DbSet<UserPostVote> UserPostVotes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserLogin>()
				.HasKey(l => new {l.Scheme, Username = l.NameIdentifier});

			modelBuilder.Entity<PostTag>()
				.HasKey(t => new { t.PostId, t.TagId });

			modelBuilder.Entity<Tag>()
				.HasIndex(tag => tag.Name)
				.IsUnique();

			modelBuilder.Entity<UserPostVote>()
				.HasKey(t => new { t.UserId, t.PostId });

			modelBuilder.Entity<SceneFile>()
				.HasIndex(sf => sf.Filename);

			modelBuilder.Entity<SupportFile>()
				.HasIndex(sf => sf.Filename);
		}
	}
}
