using System;
using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace GameClass.GameObj
{
    public static class GameObjLogger
    {
        public static readonly Logger logger = new("GameObj");
    }

    public static class LoggingFunctional
    {
        public static string CharacterLogInfo(Character character)
            => Logger.ObjInfo(typeof(Character), $"{character.TeamID} {character.PlayerID}");
        public static string CharacterLogInfo(long teamId, long characterId)
            => Logger.ObjInfo(typeof(Character), $"{teamId} {characterId}");
        public static string AutoLogInfo(object obj)
        {
            Type tp = obj.GetType();
            if (tp == typeof(Character))
                return CharacterLogInfo((Character)obj);
            else
                return Logger.ObjInfo(obj);
        }
    }

    public static class CharacterLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("Character");
    }
}

namespace GameClass.GameObj.Map
{
    public static class MapLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("Map");
    }
}
