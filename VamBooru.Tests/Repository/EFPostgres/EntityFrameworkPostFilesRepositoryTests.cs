using System;
using System.Linq;
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

		[Test]
		public async Task GetPostFiles()
		{
			var post = await GivenAPost(
				new PostFile {Filename = "My Scene.json", MimeType = "application/json", Urn = "urn:vambooru:tests:0001", Compressed = true},
				new PostFile {Filename = "My Scene.jpg", MimeType = "image/jpeg", Urn = "urn:vambooru:tests:0002"},
				new PostFile {Filename = "sound.wav", MimeType = "audio/wav", Urn = "urn:vambooru:tests:0003"}
			);

			CreateDbContext();
			var files = await Repository.LoadPostFilesAsync(post.Id);

			files.OrderBy(pf => pf.Urn).ToArray().ShouldDeepEqual(new[]
			{
				new PostFile {Filename = "My Scene.json", MimeType = "application/json", Urn = "urn:vambooru:tests:0001", Compressed = true},
				new PostFile {Filename = "My Scene.jpg", MimeType = "image/jpeg", Urn = "urn:vambooru:tests:0002"},
				new PostFile {Filename = "sound.wav", MimeType = "audio/wav", Urn = "urn:vambooru:tests:0003"}
			}, c =>
			{
				c.MaxDifferences = 3;
				c.MembersToIgnore.Add("*Id");
				c.MembersToIgnore.Add("PostFile.Post");
			});
		}

		[Test]
		public async Task GetPostFile()
		{
			var post = await GivenAPost(
				new PostFile {Filename = "file.txt", MimeType = "text/plain", Urn = "urn:vambooru:tests:0001"}
			);

			CreateDbContext();
			var file = await Repository.LoadPostFileAsync(post.Id, "urn:vambooru:tests:0001");

			file.ShouldDeepEqual(
				new PostFile {Filename = "file.txt", MimeType = "text/plain", Urn =  "urn:vambooru:tests:0001"}
				, c =>
				{
					c.MembersToIgnore.Add("*Id");
					c.MembersToIgnore.Add("PostFile.Post");
				});
		}

		private async Task<Post> GivenAPost(params PostFile[] postFiles)
		{
			var saved = await _posts.CreatePostAsync(
				LoginInfo,
				"My Post",
				new[] {"my-tag"},
				new[]
				{
					new Scene
					{
						Name = "My Scene",
					}
				},
				postFiles,
				postFiles.First().Urn,
				new DateTimeOffset(2005, 02, 03, 04, 05, 06, TimeSpan.Zero)
			);
			return saved;
		}
	}
}
