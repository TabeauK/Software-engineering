using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster
{
    public class ConfigurationLoader
    {
        private ILogger _logger;

        public ConfigurationLoader(ILogger<ConfigurationLoader> logger) => _logger = logger;

        public bool TryLoadFromFile(string fileName, out Configuration config)
        {
            try
            {
                string JSON = System.IO.File.ReadAllText(fileName);
                JsonSerializerSettings settings = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error };
                config = JsonConvert.DeserializeObject<Configuration>(JSON, settings);
                _logger.LogInformation("Successfully loaded config file: {FileName}", fileName);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Unable to load file: {Message}", e.Message);
                config = null;
                return false;
            }
        }

        public void LoadDefaultConfig(out Configuration config)
        {
            _logger.LogInformation("Loading default configuration...");
            config = new Configuration()
            {
                //CSIP = "188.208.216.232",
                CSIP = "localhost",
                //CSPort = 25570,
                CSPort = 3729,
                MovePenalty = 100,
                DestroyPenalty = 100,
                DiscoveryPenalty = 100,
                PutPenalty = 100,
                PickUpPenalty = 100,
                CheckPenalty = 100,
                InformationExchangePenalty = 500,
                RequestInformationExchangePenalty = 300,
                X = 4,
                Y = 12,
                NumberOfGoals = 2,
                NumberOfPieces = 4,
                GoalAreaHeight = 4,
                NumberOfPlayers = 2,
                ShamPieceProbability = 0.3f,
                Verbose = false
            };
            _logger.LogInformation("Default configuration loaded");
        }

        public void LoadDefaultTestConfig(out Configuration config)
        {
            _logger.LogInformation("Loading default configuration...");
            config = new Configuration()
            {
                CSIP = "localhost",
                CSPort = 3729,
                MovePenalty = 100,
                DestroyPenalty = 100,
                DiscoveryPenalty = 100,
                PutPenalty = 100,
                PickUpPenalty = 100,
                CheckPenalty = 100,
                InformationExchangePenalty = 500,
                RequestInformationExchangePenalty = 300,
                X = 3,
                Y = 3,
                NumberOfGoals = 3,
                NumberOfPieces = 1,
                GoalAreaHeight = 1,
                NumberOfPlayers = 1,
                ShamPieceProbability = 0.3f,
                Verbose = false
            };
            _logger.LogInformation("Default configuration loaded");
        }
    }
}
