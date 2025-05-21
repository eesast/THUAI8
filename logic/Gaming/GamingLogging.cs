using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace Gaming
{
    public static class GameLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("Game");
    }

    public static class ActionManagerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("ActionManager");
    }

    public static class AttackManagerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("AttackManager");
    }

    public static class CharacterManagerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("CharacterManager");
    }

    public static class A_ResourceManagerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("A_ResourceManager");
    }

    public static class SkillCastingManagerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("SkillCastingManager");
    }
}