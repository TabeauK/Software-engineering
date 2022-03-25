using System;
using System.Collections.Generic;
using System.Text;
using CommunicationUtils.Structures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Player.Models;

namespace Player.Utility
{
    public class ConfigurationLoader
    {
        private ILogger _logger;
        public ConfigurationLoader(ILogger<ConfigurationLoader> logger) => _logger = logger;
        public bool TryLoadFromArgs(string[] args, out PlayerConfiguration config)
        {
            config = null;
            int port;
            TeamColor team;
            bool verbose = false;
            bool hasVerboseInfo = false;
            try
            {
                if (args.Length != 4 && args.Length != 5) throw new Exception("Unable to load start arguments");
                bool portOK = int.TryParse(args[2], out port);
                bool teamOK = Enum.TryParse<TeamColor>(args[3], out team);
                bool verboseOK = false;
                if (args.Length == 5) hasVerboseInfo = true;
                if (hasVerboseInfo)
                {
                    verboseOK = bool.TryParse(args[4], out verbose);
                }
                if (!portOK) throw new Exception("Unable to parse port argument");
                if (!teamOK) throw new Exception("Unable to parse team argument(acceptable values: Blue or Red)");
                if (hasVerboseInfo && !verboseOK) throw new Exception("Unable to parse verbose boolean info(acceptable values: true or false)");
            }

            catch(Exception e)
            {
                _logger.LogWarning(e.Message);
                return false;
            }
            config = new PlayerConfiguration() { CSIP = args[1], CSPort = port, TeamID = team, Verbose = verbose};
            return true;
        }
        public bool TryLoadFromFile(string fileName, out PlayerConfiguration config)
        {
            try
            {
                string JSON = System.IO.File.ReadAllText(fileName);
                JsonSerializerSettings settings = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error};
                config = JsonConvert.DeserializeObject<PlayerConfiguration>(JSON, settings);
                _logger.LogInformation("Successfully loaded config file: {FileName}", fileName);
                return true;
            }
            catch(Exception e)
            {
                _logger.LogWarning("Unable to load file: {Message}", e.Message);
                config = null;
                return false;
            }
        }
        public PlayerConfiguration LoadDefaultConfig()
        {
            _logger.LogInformation("Loading default configuration...");
            return new PlayerConfiguration() { CSIP = "localhost", CSPort = 5000, TeamID = TeamColor.Blue, Verbose = false};
        }
    }
}
