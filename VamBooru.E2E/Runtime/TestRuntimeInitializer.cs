using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenQA.Selenium.Chrome;
using VamBooru.E2E.Web;

namespace VamBooru.E2E.Runtime
{
	public class TestRuntimeInitializer : IDisposable
	{
		private static readonly string BinPath = Path.GetDirectoryName(new Uri(typeof(TestRuntimeInitializer).Assembly.CodeBase).LocalPath);

		private CancellationTokenSource _token;
		private Task _task;
		private ChromeDriver _driver;

		public void Initialize()
		{
			OverrideEnvironmentDirectory();

			var host = TestWebHostBuilderFactory.CreateWebHostBuilder(new string[0]).Build();

			_token = new CancellationTokenSource();
			_task = Task.Run(async () => await host.RunAsync(_token.Token));
			_driver = CreateChromeDriver();

			DoAndWait(
				"Acquiring host address",
				() => host.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses?.FirstOrDefault(),
				address => address != null,
				address => TestRuntime.InitializeServer(new Uri(address)),
				_ => "address was null"
			);

			DoAndWait(
				"First ping",
				() => TestRuntime.HttpClient.GetAsync("/api/ping").ConfigureAwait(false).GetAwaiter().GetResult(),
				response => response.StatusCode == HttpStatusCode.OK,
				response => { },
				response => response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult()
			);

			DoAndWait(
				"Home page up",
				() => TestRuntime.HttpClient.GetAsync("/").ConfigureAwait(false).GetAwaiter().GetResult(),
				response => response.StatusCode == HttpStatusCode.OK,
				response => { },
				response => response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult()
			);

			DoAndWait(
				"Open browser on the home page",
				() =>
				{
					TestRuntime.InitializeDriver(_driver);
					TestRuntime.Pages.Home.Go();
					return TestRuntime.Pages;
				},
				pages => pages.Title() == "VamBooru",
				pages => { },
				pages => $"Title: {pages.Title()}\nSource:\n{pages.Source()}"
			);
		}

		private static void OverrideEnvironmentDirectory()
		{
			var dir = Path.Combine(BinPath, "..", "..", "..");
			dir = Path.GetFullPath(new Uri(dir).LocalPath);
			Environment.CurrentDirectory = dir;
		}

		private static ChromeDriver CreateChromeDriver()
		{
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArguments("headless");

			var chromeDriverService = ChromeDriverService.CreateDefaultService(BinPath);
			ChromeDriver driver = new ChromeDriver(chromeDriverService, chromeOptions);
			return driver;
		}

		private void DoAndWait<T>(string name, Func<T> acquire, Func<T, bool> test, Action<T> act, Func<T, string> error)
		{
			var timer = new Stopwatch();
			timer.Start();
			while (true)
			{
				if (_task.IsCanceled || _task.IsFaulted)
					throw new TaskCanceledException(_task);

				var value = acquire();

				if (!test(value))
				{
					if (timer.Elapsed > TimeSpan.FromSeconds(30))
						throw new TimeoutException($"Could not finish step in alloted time: {name}\n{error(value)}");

					Thread.Sleep(50);
					continue;
				}

				act(value);
				Debug.WriteLine($"{name} finished in {timer.Elapsed.TotalSeconds:0.00} seconds: {value}");
				return;
			}
		}

		public void Dispose()
		{
			_driver?.Close();
			_driver?.Dispose();
			_token?.Cancel();
			_task?.Wait();
			_task?.Dispose();
		}
	}
}
