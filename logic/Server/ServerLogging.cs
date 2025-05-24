using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace Server
{
    public static class GameServerLogging
    {
        public static readonly AdvancedLoggerFactory.Logger logger = AdvancedLoggerFactory.CreateLogger("GameServer");
    }

    public static class PlaybackServerLogging
    {
        public static readonly AdvancedLoggerFactory.Logger logger = AdvancedLoggerFactory.CreateLogger("PlaybackServer");
    }
}