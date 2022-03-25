using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Player.Models.Strategies
{
    public class DefaultStrategy : IStrategy
    {
        private ILogger _logger;
        public DefaultStrategy(ILogger<DefaultStrategy> logger)
        {
            _logger = logger;
        }

        public void Init(Player player)
        {
            return;
        }

        public void MakeDecision()
        {
            _logger.LogInformation("DefaultStrategy making decision");
        }
    }
}
