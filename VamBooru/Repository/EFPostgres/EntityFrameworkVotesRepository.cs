using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository.EFPostgres
{
	public class EntityFrameworkVotesRepository : EntityFrameworkRepositoryBase, IVotesRepository
	{
		private readonly IUsersRepository _users;

		public EntityFrameworkVotesRepository(VamBooruDbContext dbContext, IUsersRepository users)
			: base(dbContext)
		{
			_users = users ?? throw new ArgumentNullException(nameof(users));
		}

		public async Task<UserPostVote> GetVoteAsync(UserLoginInfo login, Guid postId)
		{
			var dbUser = await _users.LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			return await DbContext.UserPostVotes.Where(upv => upv.User == dbUser && upv.PostId == postId).FirstOrDefaultAsync();
		}

		public async Task<int> VoteAsync(UserLoginInfo login, Guid postId, int votes)
		{
			var dbUser = await _users.LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			var dbVote = await DbContext.UserPostVotes.Where(upv => upv.User == dbUser && upv.PostId == postId).FirstOrDefaultAsync();
			int difference;
			if (dbVote == null)
			{
				difference = votes;
				if (votes != 0)
				{
					dbVote = new UserPostVote {PostId = postId, User = dbUser, Votes = votes};
					DbContext.UserPostVotes.Add(dbVote);
				}
			}
			else
			{
				difference = votes - dbVote.Votes;
				if (votes == 0)
					DbContext.UserPostVotes.Remove(dbVote);
				else
					dbVote.Votes = votes;
			}

			if (difference != 0)
				await DbContext.Database.ExecuteSqlCommandAsync(
					"UPDATE \"Posts\" SET \"Votes\" = \"Votes\" + @difference WHERE \"Id\" = @postId",
					new NpgsqlParameter("@difference", difference),
					new NpgsqlParameter("@postId", postId)
				);

			await DbContext.SaveChangesAsync();

			return difference;
		}

		public Task<UserPostVote[]> GetPostVotingUsers(Guid postId)
		{
			return DbContext.UserPostVotes
				.AsNoTracking()
				.Include(upv => upv.User)
				.Where(upv => upv.PostId == postId)
				.ToArrayAsync();
		}

		public async Task<UserPostVote[]> GetUserVotedPosts(UserLoginInfo login)
		{
			var dbUser = await _users.LoadPrivateUserAsync(login) ?? throw new NullReferenceException("User does not exist");
			return await DbContext.UserPostVotes
				.AsNoTracking()
				.Include(upv => upv.Post)
				.Where(upv => upv.User == dbUser)
				.Select(upv => new UserPostVote
				{
					Votes = upv.Votes,
					PostId = upv.PostId,
					UserId = upv.UserId,
					Post = new Post
					{
						Id = upv.Post.Id,
						Title = upv.Post.Title,
						Votes = upv.Post.Votes
					}
				})
				.ToArrayAsync();
		}
	}
}
