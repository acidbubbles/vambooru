using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/votes")]
	public class VotesController : Controller
	{
		private readonly IRepository _repository;

		public VotesController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("{postId}")]
		public async Task<IActionResult> GetVote([FromRoute] Guid postId)
		{
			var userVote = await _repository.GetVoteAsync(this.GetUserLoginInfo(), postId);

			var value = Math.Clamp(userVote?.Votes ?? 0, -1, 1);

			return Json(new VoteValue
			{
				Value = value
			});
		}

		[HttpPost("{postId}")]
		public async Task<IActionResult> SetVote([FromRoute] Guid postId, [FromBody] VoteValue voteValue)
		{
			if (voteValue == null) return BadRequest();
			var value = voteValue.Value;
			if (value < -1 || value > 1) return BadRequest();

			// Super simple voting system. Voting up is worth more than voting down.
			var votes = 0;
			if (value == -1) votes = -2;
			if (value == 1) votes = 10;

			var difference = await _repository.VoteAsync(this.GetUserLoginInfo(), postId, votes);

			return Json(new
			{
				Difference = difference
			});
		}

		public class VoteValue
		{
			public int Value { get; set; }
		}
	}
}
