using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace CommunicationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                Server communicationServer = new Server();
                if (args.Length > 1)
                    communicationServer.Launch(args);
                else
                    communicationServer.Launch("config.json");

                Console.WriteLine("Press q to stop the server");
                while(communicationServer.IsRunning())
                {
                    ConsoleKeyInfo c = Console.ReadKey();
                    switch(c.Key)
                    {
                        case ConsoleKey.Q:
                            communicationServer.Stop();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddSerilog();
            });
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($"CS_{DateTime.Now.ToString("MM-dd_HH-mm-ss")}.log")
                .WriteTo.Console()
                .WriteTo.Debug()
                .CreateLogger();

            //services.AddSingleton(Log.Logger);
        }
    }
}
