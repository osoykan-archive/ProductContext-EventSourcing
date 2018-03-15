using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using ProductContext.WebApi.Plumbing;

using Serilog;
using Serilog.Events;

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
                   .ConfigureLogging((context, builder) =>
                   {
                       LoggerConfiguration loggerCfg = new LoggerConfiguration()
                           .MinimumLevel.Debug()
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                           .Enrich.FromLogContext();

                       if (context.HostingEnvironment.IsLocal())
                       {
                           loggerCfg
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Trace\{Date}.txt", LogEventLevel.Verbose, shared: true)
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Debug\{Date}.txt", LogEventLevel.Debug, shared: true)
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Information\{Date}.txt", LogEventLevel.Information, shared: true)
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Warning\{Date}.txt", LogEventLevel.Warning, shared: true)
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Error\{Date}.txt", LogEventLevel.Error, shared: true)
                                    .WriteTo.RollingFile(context.Configuration["Logging:LocalPath"] + @"\Fatal\{Date}.txt", LogEventLevel.Fatal, shared: true);
                       }

                       Log.Logger = loggerCfg.CreateLogger();
                       builder.AddSerilog(Log.Logger);
                   })
                   .UseKestrel()
                   .Build();
    }
}
