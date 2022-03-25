using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Serilog;
using Player.Models;
using Player.Models.Strategies;
using Player.Utility;
using CommunicationUtils;

namespace Player
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                Player player = serviceProvider.GetService<Player>();
                player.Start(args);
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
                .WriteTo.File($"Player_{DateTime.Now.ToString("MM-dd_HH-mm-ss-ffffff")}.log")
                .CreateLogger();
            services.AddSingleton<IStrategy, FastDiscoverAndCheck>();
            //services.AddSingleton<ICommunicator, Communicator>();
            services.AddSingleton<ConfigurationLoader>();
            services.AddSingleton<Player>();
        }
    }
}
