using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VamBooru.Models;
using VamBooru.Services;

namespace VamBooru.Controllers
{
	[Route("api/scenes")]
	public class ScenesController : Controller
	{
		private readonly IRepository _repository;

		public ScenesController(IRepository repository)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
		}

		[HttpGet("")]
		public Task<Scene[]> Browse()
		{
			return _repository.BrowseScenesAsync();
		}

		[HttpGet("{id}")]
		public Task<Scene> Get(Guid id)
		{
			return _repository.LoadSceneAsync(id);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Save([FromBody] Scene scene)
		{
			await _repository.UpdateSceneAsync(scene);
			return NoContent();
		}
	}
}
