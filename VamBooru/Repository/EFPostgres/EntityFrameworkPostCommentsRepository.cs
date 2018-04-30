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
			var post = new Post {Id = postId};
			var comment = new PostComment
			{
				Author = user,
				DateCreated = now,
				Post = post,
				Text = text
			};
			DbContext.PostComments.Add(comment);
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
