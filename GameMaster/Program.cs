using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunicationUtils;
using Serilog;
using System;

namespace GameMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                GameMaster gameMaster = serviceProvider.GetService<GameMaster>();
                gameMaster.Start();
                
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                //configure.AddConsole();
                configure.AddSerilog();
            });
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File($"GM_{DateTime.Now.ToString("MM-dd_HH-mm-ss")}.log")
                .WriteTo.Console()
                .CreateLogger();
            services.AddSingleton<ConfigurationLoader>();
            services.AddSingleton<GameMaster>();
            //services.AddSingleton<Communicator>();
        }
    }
}
