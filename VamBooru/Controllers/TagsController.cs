using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Repository;
using VamBooru.ViewModels;

namespace VamBooru.Controllers
{
	[Route("api/tags")]
	public class TagsController
	{
		private readonly ITagsRepository _tagsRepository;

		public TagsController(ITagsRepository tagsRepository)
		{
			_tagsRepository = tagsRepository ?? throw new ArgumentNullException(nameof(tagsRepository));
		}

		[HttpGet("")]
		public async Task<TagViewModel[]> SearchTags([FromQuery] string q)
		{
			if (string.IsNullOrWhiteSpace(q)) return new TagViewModel[0];
			var tags = await _tagsRepository.SearchTags(q);
			return tags.Select(TagViewModel.From).ToArray();
		}


		[HttpGet("top")]
		public async Task<TagViewModel[]> LoadTopTags()
		{
			var tags = await _tagsRepository.LoadTopTags(16);
			return tags.Select(TagViewModel.From).ToArray();
		}
	}
}
