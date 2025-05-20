using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace Server
{
    public static class GameServerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("GameServer");
    }

    public static class PlaybackServerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("PlaybackServer");
    }
}