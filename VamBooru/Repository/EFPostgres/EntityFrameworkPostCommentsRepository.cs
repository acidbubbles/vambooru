using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkPostCommentsRepository : EntityFrameworkRepositoryBase, IPostCommentsRepository
	{
		private readonly IUsersRepository _usersRepository;

		public EntityFrameworkPostCommentsRepository(VamBooruDbContext dbContext, IUsersRepository usersRepository)
			: base(dbContext)
		{
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
		}

		public async Task<PostComment> CreatePostCommentAsync(UserLoginInfo login, Guid postId, string text, DateTimeOffset now)
		{
			var user = await _usersRepository.LoadPrivateUserAsync(login);
			if (user == null) throw new NullReferenceException("No user matching the specified login");
			var comment = new PostComment
			{
				Author = user,
				DateCreated = now,
				Text = text
			};
			DbContext.Attach(comment);
			DbContext.Entry(comment).Property<Guid>("PostId").CurrentValue = postId;
			await DbContext.SaveChangesAsync();
			return comment;
		}

		public Task<PostComment[]> LoadPostCommentsAsync(Guid postId)
		{
			return DbContext.PostComments
				.Where(pc => pc.Post.Id == postId)
				.Include(pc => pc.Author)
				.OrderByDescending(pc => pc.DateCreated)
				.ToArrayAsync();
		}
	}
}
