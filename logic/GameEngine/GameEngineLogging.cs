using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace GameEngine
{
    public static class GameEngineLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("GameEngine");
    }
}
