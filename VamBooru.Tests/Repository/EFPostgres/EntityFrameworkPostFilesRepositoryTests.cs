using System;
using System.Threading.Tasks;
using NUnit.Framework;
using VamBooru.Models;
using VamBooru.Repository.EFPostgres;

namespace VamBooru.Tests.Repository.EFPostgres
{
	public class EntityFrameworkPostFilesRepositoryTests : EntityFrameworkRepositoryTestsBase<EntityFrameworkPostFilesRepository>
	{
		private EntityFrameworkPostsRepository _posts;

		[SetUp]
		public async Task BeforeEach()
		{
			await Initialize();
			await CreateUser();
		}

		[TearDown]
		public void AfterEach()
		{
			Cleanup();
		}

		protected override EntityFrameworkPostFilesRepository Create(VamBooruDbContext context)
		{
			_posts = new EntityFrameworkPostsRepository(context, new EntityFrameworkUsersRepository(context));
			return new EntityFrameworkPostFilesRepository(context);
		}

		public async Task GetPostFiles()
		{
			var saved = await _posts.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene"
					}
				},
				new[]
				{
					new PostFile {Filename = "My Scene.json", Urn = "urn:vambooru:tests:0001", Compressed = true},
					new PostFile {Filename = "My Scene.jpg", Urn = "urn:vambooru:tests:0002"},
					new PostFile {Filename = "sound.wav", Urn = "urn:vambooru:tests:0003"},
				},
				"urn:vambooru:tests:0002",
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);

			CreateDbContext();
			var files = await Repository.LoadPostFilesAsync(saved.Id);

			files.ShouldDeepEqual(new[]
			{
				new PostFile {Filename = "My Scene.json"},
				new PostFile {Filename = "My Scene.jpg"},
				new PostFile {Filename = "sound.wav"}
			}, c =>
			{
				c.IgnoreCollectionOrder = true;
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("PostFile.Post");
			});
		}
	}
}
