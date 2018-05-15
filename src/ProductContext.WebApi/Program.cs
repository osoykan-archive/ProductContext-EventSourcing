using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ProductContext.WebApi
{
	public class Program
	{
		public static int Main(string[] args)
		{
			BuildWebHost(args).Run();
			return 0;
		}

		private static IWebHost BuildWebHost(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
			       .UseStartup<Startup>()
			       .UseKestrel()
			       .Build();
	}
}
