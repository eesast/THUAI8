namespace Preparation.Utility
{
    public enum PlaceType
    {
        NULL_PLACE_TYPE = 0,
        HOME = 1,              // 出生点（地图左下与右上）
        SPACE = 2,             // 空地
        BARRIER = 3,           // 障碍
        BUSH = 4,              // 草丛
        ECONOMY_RESOURCE = 5,  // 经济资源
        ADDITION_RESOURCE = 6, // 加成资源
        CONSTRUCTION = 7,      // 建筑
        TRAP = 8,              // 陷阱
    }

    public enum GameObjType : uint
    {
        NULL = 0,
        CHARACTER = 1,

        BARRIER = 2,
        BUSH = 3,
        ECONOMY_RESOURCE = 4,//经济资源
        ADDITIONAL_RESOURCE = 5,//加成资源
        CONSTRUCTION = 6,
        TRAP = 7,
        HOME = 8,
        OUTOFBOUNDBLOCK = 9,
        SPACE = 10,
    }

    public enum A_ResourceType
    {
        NULL = 0,
        CRAZY_MAN1 = 1,
        CRAZY_MAN2 = 2,
        CRAZY_MAN3 = 3,
        LIFE_POOL1 = 4,
        LIFE_POOL2 = 5,
        LIFE_POOL3 = 6,
        QUICK_STEP = 7,
        WIDE_VIEW = 8
    }
    public enum CharacterState //角色状态
    {
        NULL_CHARACTER_STATE = 0,

        // 主动状态
        IDLE = 1,
        HARVESTING = 2,
        ATTACKING = 3,
        SKILL_CASTING = 4,
        CONSTRUCTING = 5,
        MOVING = 6,

        // 被动状态
        BLIND = 7,
        KNOCKED_BACK = 8,
        STUNNED = 9,
        INVISIBLE = 10,
        HEALING = 11,
        BERSERK = 12,
        BURNED = 13,
        DECEASED = 14,
    }

    public enum ShapeType
    {
        NULL_SHAPE_TYPE = 0,
        CIRCLE = 1,
        SQUARE = 2,
    }

    public enum EconomyResourceState //经济资源状态
    {
        NULL_ECONOMY_RESOURCE_STATE = 0,
        HARVESTABLE = 1,
        BEING_HARVESTED = 2,
        HARVESTED = 3,
    }
    public enum EconomyResourceType // 经济资源
    {
        NULL_ECONOMY_RESOURCE_TYPE = 0,

        SMALL_ECONOMY_RESOURCE = 1,
        MEDIUM_ECONOMY_RESOURCE = 2,
        LARGE_ECONOMY_RESOURCE = 3,
    }

    public enum AdditionResourceState //加成资源状态
    {
        NULL_ADDITION_RESOURCE_STATE = 0,
        BEATABLE = 1,
        BEING_BEATEN = 2,
        BEATEN = 3,
    }

    public enum EquipmentType //装备类型
    {
        NULL_EQUIPMENT_TYPE = 0,

        SMALL_HEALTH_POTION = 1,
        MEDIUM_HEALTH_POTION = 2,
        LARGE_HEALTH_POTION = 3,

        SMALL_SHIELD = 4,
        MEDIUM_SHIELD = 5,
        LARGE_SHIELD = 6,

        SPEEDBOOTS = 7,
        PURIFICATION_POTION = 8,
        INVISIBILITY_POTION = 9,
        BERSERK_POTION = 10,
    }

    public enum ConstructionType
    {
        NULL_CONSTRUCTION_TYPE = 0,
        BARRACKS = 1,
        SPRING = 2,
        FARM = 3,
        HOLE = 4,
        CAGE = 5,
    }

    public enum TrapType
    {
        NULL_TRAP_TYPE = 0,
        HOLE = 1, // 坑洞陷阱
        CAGE = 2, // 牢笼陷阱
    }

    public enum CharacterType
    {
        Null = 0,

        //取经团队阵营
        TangSeng = 1,
        SunWukong = 2,
        ZhuBajie = 3,
        ShaWujing = 4,
        BaiLongma = 5,

        //妖怪阵营
        JiuLing = 6,
        HongHaier = 7,
        NiuMowang = 8,
        TieShan = 9,
        ZhiZhujing = 10,

        Monkid = 11,
        Pawn = 12,
    }
}
