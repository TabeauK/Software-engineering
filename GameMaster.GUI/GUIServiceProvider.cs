using CommunicationUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameMaster.GUI
{
    static class GUIServiceProvider
    {
        public static GameMaster GameMaster;

        public static void ConfigureGMServices(ServiceCollection services)
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
