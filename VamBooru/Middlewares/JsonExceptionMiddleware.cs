using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace VamBooru.Middlewares
{
	public class JsonExceptionMiddleware
	{
		private readonly bool _isDev;

		public JsonExceptionMiddleware(IHostingEnvironment env)
		{
			_isDev = env.IsDevelopment();
		}

		public async Task Invoke(HttpContext context)
		{
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

			var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
			if (ex == null) return;

			context.Response.ContentType = "text/plain";

			if (_isDev)
				await context.Response.WriteAsync(ex.ToString());
			else
				await context.Response.WriteAsync("An error occurred, sorry about that!");
		}
	}
}
