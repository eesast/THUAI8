using Preparation.Utility.Logging;

namespace Gaming
{
    public static class GameLogging
    {
        public static readonly Logger logger = new("Game");
    }

    public static class ActionManagerLogging
    {
        public static readonly Logger logger = new("ActionManager");
    }

    public static class AttackManagerLogging
    {
        public static readonly Logger logger = new("AttackManager");
    }

    public static class CharacterManagerLogging
    {
        public static readonly Logger logger = new("CharacterManager");
    }

    public static class A_ResourceManagerLogging
    {
        public static readonly Logger logger = new("A_ResourceManager");
    }

    public static class SkillCastingManagerLogging
    {
        public static readonly Logger logger = new("SkillCastingManager");
    }
}