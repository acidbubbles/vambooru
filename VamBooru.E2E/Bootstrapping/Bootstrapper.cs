using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using VamBooru.Models;
using VamBooru.Repository;
using VamBooru.Storage;
using VamBooru.Tests.Repository.EFPostgres;
using VamBooru.ViewModels;

namespace VamBooru.E2E.Bootstrapping
{
	public class Bootstrapper
	{
		private readonly IHostingEnvironment _env;
		private readonly VamBooruDbContext _dbContext;
		private readonly IUsersRepository _usersRepository;
		private readonly IPostsRepository _postsRepository;
		private readonly IPostCommentsRepository _commentsRepository;
		private readonly IVotesRepository _votesRepository;
		private readonly IStorage _storage;

		public Bootstrapper(IHostingEnvironment env, VamBooruDbContext dbContext, IUsersRepository usersRepository, IPostsRepository postsRepository, IPostCommentsRepository commentsRepository, IVotesRepository votesRepository, IStorage storage)
		{
			_env = env ?? throw new ArgumentNullException(nameof(env));
			_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			_usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
			_postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
			_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			_commentsRepository = commentsRepository ?? throw new ArgumentNullException(nameof(commentsRepository));
			_votesRepository = votesRepository ?? throw new ArgumentNullException(nameof(votesRepository));
		}

		public async Task Seed()
		{
			// Clear database
			await EntityFrameworkTestsHelper.ClearAndMarkTestDatabase(_dbContext);

			// Setup users
			var cloudyLogin = new UserLoginInfo("e2e", "cloudy_dude");
			var cloudy = await _usersRepository.LoadOrCreateUserFromLoginAsync(cloudyLogin.Scheme, cloudyLogin.NameIdentifier, "Cloudy Dude", new DateTimeOffset(2018, 01, 15, 15, 23, 12, TimeSpan.Zero));
			var billyLogin = new UserLoginInfo("e2e", "H4x0rs4evaaaah");
			var billy = await _usersRepository.LoadOrCreateUserFromLoginAsync(billyLogin.Scheme, billyLogin.NameIdentifier, "Billy", new DateTimeOffset(2000, 01, 01, 00, 00, 00, TimeSpan.Zero));
			var edLogin = new UserLoginInfo("e2e", "153234");
			var ed = await _usersRepository.LoadOrCreateUserFromLoginAsync(edLogin.Scheme, edLogin.NameIdentifier, "ed", new DateTimeOffset(2018, 03, 14, 19, 54, 48, TimeSpan.Zero));

			// Setup posts
			var gettingReadyToJumpJpg = await CreateFile(new PostFile
			{
				Filename = "getting_ready_to_jump.jpg",
				MimeType = "image/jpeg",
				Compressed = false,
			});
			var gettingReadyToJumpJson = await CreateFile(new PostFile
			{
				Filename = "getting_ready_to_jump.json",
				MimeType = "application/json",
				Compressed = true,
			});
			var gettingReadyToJumpWav = await CreateFile(new PostFile
			{
				Filename = "getting_ready_to_jump.wav",
				MimeType = "audio/wav",
				Compressed = true,
			});
			var gettingReadyToJumpPost = await _postsRepository.CreatePostAsync(
				cloudyLogin,
				"Jumping Lady",
				new[] {"jumping", "lady"},
				new[]
				{
					new Scene
					{
						Name = "getting_ready_to_jump",
						ThumbnailUrn = gettingReadyToJumpJpg.Urn
					}
				},
				new[]
				{
					gettingReadyToJumpJson, gettingReadyToJumpJpg, gettingReadyToJumpWav
				},
				gettingReadyToJumpJpg.Urn,
				new DateTimeOffset(2018, 01, 16, 21, 02, 54, TimeSpan.Zero)
			);
			var gettingReadyToJumpPostViewModel = PostViewModel.From(gettingReadyToJumpPost, false);
			gettingReadyToJumpPostViewModel.Published = true;
			await _postsRepository.UpdatePostAsync(cloudyLogin, gettingReadyToJumpPostViewModel, new DateTimeOffset(2018, 01, 16, 21, 03, 49, TimeSpan.Zero));

			// Setup comments
			await _commentsRepository.CreatePostCommentAsync(billyLogin, gettingReadyToJumpPost.Id, "This scene rocks man", new DateTimeOffset(2018, 01, 16, 21, 02, 54, TimeSpan.Zero));
			await _commentsRepository.CreatePostCommentAsync(cloudyLogin, gettingReadyToJumpPost.Id, "Thanks!", new DateTimeOffset(2018, 01, 16, 21, 04, 24, TimeSpan.Zero));
			await _commentsRepository.CreatePostCommentAsync(edLogin, gettingReadyToJumpPost.Id, "She's not even *jumping*, _yo_.", new DateTimeOffset(2018, 01, 16, 21, 08, 07, TimeSpan.Zero));
			await _commentsRepository.CreatePostCommentAsync(billyLogin, gettingReadyToJumpPost.Id, "Get\na\nlife", new DateTimeOffset(2018, 01, 16, 21, 12, 35, TimeSpan.Zero));

			// Setup votes
			await _votesRepository.VoteAsync(cloudyLogin, gettingReadyToJumpPost.Id, 10);
			await _votesRepository.VoteAsync(billyLogin, gettingReadyToJumpPost.Id, 10);
			await _votesRepository.VoteAsync(edLogin, gettingReadyToJumpPost.Id, -2);
		}

		private async Task<PostFile> CreateFile(PostFile fileInfo)
		{
			if(fileInfo.Filename.IndexOfAny(Path.GetInvalidFileNameChars()) > 0) throw new UnauthorizedAccessException();

			var source = Path.Combine(_env.ContentRootPath, "..", "VamBooru.E2E", "Bootstrapping", "Files", fileInfo.Filename);

			var file = fileInfo;
			using (var fileStream = File.OpenRead(source))
			using (var memoryStream = new MemoryStream())
			{
				await fileStream.CopyToAsync(memoryStream);
				file.Urn = await _storage.SaveFileAsync(memoryStream, fileInfo.Compressed);
			}
			return file;
		}
	}
}
