using System;
using System.Threading.Tasks;
using VamBooru.Models;
using VamBooru.ViewModels;

namespace VamBooru.Repository
{
	public interface IVotesRepository
	{
		Task<UserPostVote> GetVoteAsync(UserLoginInfo login, Guid postId);
		Task<int> VoteAsync(UserLoginInfo login, Guid postId, int votes);
	}
}
