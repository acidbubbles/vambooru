using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LightBDD.NUnit3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using VamBooru.E2E.Steps;
using VamBooru.E2E.Web;

[assembly: ConfiguredLightBddScope]

namespace VamBooru.E2E.Steps
{
	public class ConfiguredLightBddScopeAttribute : LightBddScopeAttribute
	{
		private CancellationTokenSource _token;
		private Task _task;

		protected override void OnSetUp()
		{
			OverrideEnvironmentDirectory();

			var host = TestWebHostBuilderFactory.CreateWebHostBuilder(new string[0]).Build();

			_token = new CancellationTokenSource();
			_task = Task.Run(async () => await host.RunAsync(_token.Token));

			DoAndWait(
				"Acquiring host address",
				() => host.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses?.FirstOrDefault(),
				address => address != null,
				address => TestRuntime.Initialize(new Uri(address)),
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
		}

		private static void OverrideEnvironmentDirectory()
		{
			var dir = Path.Combine(Path.GetDirectoryName(new Uri(typeof(TestStartup).Assembly.CodeBase).LocalPath), "..", "..", "..");
			dir = Path.GetFullPath(new Uri(dir).LocalPath);
			Environment.CurrentDirectory = dir;
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

		protected override void OnTearDown()
		{
			_token?.Cancel();
			_task?.Wait();
		}
	}
}
