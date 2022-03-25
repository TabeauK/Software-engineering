using Microsoft.VisualStudio.TestTools.UnitTesting;
using Player.Models;
using Player.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using CommunicationUtils.Structures;

namespace Player.Test
{
    [TestClass]
    public class ConfigTests
    {
        private ConfigurationLoader _configLoader;
        [TestInitialize]
        public void Setup()
        {
            ILogger<ConfigurationLoader> logger = new Logger<ConfigurationLoader>(new NullLoggerFactory());
            _configLoader = new ConfigurationLoader(logger);
        }
        [TestMethod]
        public void LoadFromFileNoFile()
        {
            // Given
            string fileName = "testFile";
            PlayerConfiguration config;
            System.IO.File.Delete(fileName);

            // When
            bool result = _configLoader.TryLoadFromFile(fileName, out config);

            // Then
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void LoadFromFileBadFormat()
        {
            // Given
            string fileName = "testFile";
            PlayerConfiguration config;
            object data = new { id = 5, Napisik = "1234" };
            string json = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(fileName, json);

            // When
            bool result = _configLoader.TryLoadFromFile(fileName, out config);

            // Then
            Assert.IsFalse(result);
        }
        [TestMethod]
        public void LoadFromFileGoodFileWithVerbose()
        {
            // Given
            string fileName = "testFile";
            PlayerConfiguration config;
            object data = new { CSIP = "192.168.0.1", CSPort = 3729, TeamID = "Blue", Verbose = true};
            string json = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(fileName, json);
            PlayerConfiguration expectedConfig = new PlayerConfiguration() { CSIP = "192.168.0.1", CSPort = 3729, TeamID = TeamColor.Blue, Verbose = true}; 

            // When
            bool result = _configLoader.TryLoadFromFile(fileName, out config);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(expectedConfig.TeamID, config.TeamID);
            Assert.AreEqual(expectedConfig.CSPort, config.CSPort);
            Assert.AreEqual(expectedConfig.CSIP, config.CSIP);
            Assert.AreEqual(expectedConfig.Verbose, config.Verbose);
        }

        [TestMethod]
        public void LoadFromFileGoodFileWithoutVerbose()
        {
            // Given
            string fileName = "testFile";
            PlayerConfiguration config;
            object data = new { CSIP = "192.168.0.1", CSPort = 3729, TeamID = "Blue"};
            string json = JsonConvert.SerializeObject(data);
            System.IO.File.WriteAllText(fileName, json);
            PlayerConfiguration expectedConfig = new PlayerConfiguration() { CSIP = "192.168.0.1", CSPort = 3729, TeamID = TeamColor.Blue, Verbose = false };

            // When
            bool result = _configLoader.TryLoadFromFile(fileName, out config);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(expectedConfig.TeamID, config.TeamID);
            Assert.AreEqual(expectedConfig.CSPort, config.CSPort);
            Assert.AreEqual(expectedConfig.CSIP, config.CSIP);
            Assert.AreEqual(expectedConfig.Verbose, config.Verbose);
        }
        [TestMethod]
        public void LoadFromInputArgsGoodArgsWithVerbose()
        {
            // Given
            string[] args = new string[] { "application", "localhost", "5000", "Blue", "true" };
            PlayerConfiguration config;
            PlayerConfiguration expectedConfig = new PlayerConfiguration() { CSIP = "localhost", CSPort = 5000, TeamID = TeamColor.Blue, Verbose = true };

            // When
            bool result = _configLoader.TryLoadFromArgs(args, out config);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(expectedConfig.CSIP, config.CSIP);
            Assert.AreEqual(expectedConfig.CSPort, config.CSPort);
            Assert.AreEqual(expectedConfig.TeamID, config.TeamID);
            Assert.AreEqual(expectedConfig.Verbose, config.Verbose);
        }
        [TestMethod]
        public void LoadFromInputArgsGoodArgsWithoutVerbose()
        {
            // Given
            string[] args = new string[] { "application", "localhost", "5000", "Blue"};
            PlayerConfiguration config;
            PlayerConfiguration expectedConfig = new PlayerConfiguration() { CSIP = "localhost", CSPort = 5000, TeamID = TeamColor.Blue, Verbose = false };

            // When
            bool result = _configLoader.TryLoadFromArgs(args, out config);

            // Then
            Assert.IsTrue(result);
            Assert.AreEqual(expectedConfig.CSIP, config.CSIP);
            Assert.AreEqual(expectedConfig.CSPort, config.CSPort);
            Assert.AreEqual(expectedConfig.TeamID, config.TeamID);
            Assert.AreEqual(expectedConfig.Verbose, config.Verbose);
        }
    }
}