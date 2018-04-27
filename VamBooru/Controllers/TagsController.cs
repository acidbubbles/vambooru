using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/tags")]
	public class TagsController
	{
		private readonly IRepository _repository;

		public TagsController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("")]
		public async Task<TagViewModel[]> SearchTags([FromQuery] string q)
		{
			if (string.IsNullOrWhiteSpace(q)) return new TagViewModel[0];
			var tags = await _repository.SearchTags(q);
			return tags.Select(TagViewModel.From).ToArray();
		}


		[HttpGet("top")]
		public async Task<TagViewModel[]> LoadTopTags()
		{
			var tags = await _repository.LoadTopTags(16);
			return tags.Select(TagViewModel.From).ToArray();
		}
	}
}
