import sys
from enum import Enum
from typing import Dict, List

if sys.version_info < (3, 9):
    from typing import Tuple
else:
    Tuple = tuple


class GameState(Enum):
    NullGameState = 0
    GameStart = 1
    GameRunning = 2
    GameEnd = 3


class PlaceType(Enum):
    NullPlaceType = 0
    Home = 1
    Space = 2
    Barrier = 3
    Bush = 4
    EconomyResource = 5
    AdditionResource = 6
    Construction = 7
    Trap = 8


class ShapeType(Enum):
    NullShapeType = 0
    Circle = 1
    Square = 2


class PlayerTeam(Enum):
    NullTeam = 0
    BuddhistsTeam = 1
    MonstersTeam = 2


class PlayerType(Enum):
    NullPlayerType = 0
    Character = 1
    Team = 2


class CharacterType(Enum):
    NullCharacterType = 0
    TangSeng = 1
    SunWukong = 2
    ZhuBajie = 3
    ShaWujing = 4
    BaiLongma = 5
    Monkid = 6
    JiuLing = 7
    HongHaier = 8
    NiuMowang = 9
    TieShan = 10
    ZhiZhujing = 11
    Pawn = 12


class EquipmentType(Enum):
    NullEquipmentType = 0

    SmallHealthPotion = 1
    MediumHealthPotion = 2
    LargeHealthPotion = 3

    SmallShield = 4
    MediumShield = 5
    LargeShield = 6

    Speedboots = 7
    PurificationPotion = 8
    InvisibilityPotion = 9
    BerserkPotion = 10


class CharacterState(Enum):
    NullCharacterState = 0

    Idle = 1
    Harvesting = 2
    Attacking = 3
    SkillCasting = 4
    Constructing = 5
    Moving = 6

    Blind = 7
    KnockedBack = 8
    Stunned = 9
    Invisible = 10
    Healing = 11
    Berserk = 12
    Burned = 13
    Deceased = 14


class CharacterBuffType(Enum):
    NullCharacterBuffType = 0

    AttackBuff1 = 1
    AttackBuff2 = 2
    AttackBuff3 = 3
    DefenseBuff = 4
    SpeedBuff = 5
    VisionBuff = 6


class EconomyResourceType(Enum):
    NullEconomyResourceType = 0

    SmallEconomyResource = 1
    MediumEconomyResource = 2
    LargeEconomyResource = 3


class AdditionResourceType(Enum):
    NullAdditionResourceType = 0

    LifePool1 = 1
    LifePool2 = 2
    LifePool3 = 3

    CrazyMan1 = 4
    CrazyMan2 = 5
    CrazyMan3 = 6

    QuickStep = 7

    WideView = 8


class EconomyResourceState(Enum):
    NullEconomyResourceState = 0
    Harvestable = 1
    BeingHarvested = 2
    Harvested = 3


class AdditionResourceState(Enum):
    NullAdditionResourceState = 0
    Beatable = 1
    BeingBeaten = 2
    Beaten = 3


class ConstructionType(Enum):
    NullConstructionType = 0
    Barracks = 1
    Spring = 2
    Farm = 3


class TrapType(Enum):
    NullTrapType = 0
    Hole = 1
    Cage = 2


class MessageOfObj(Enum):
    NullMessageOfObj = 0
    CharacterMessage = 1
    BarracksMessage = 2
    SpringMessage = 3
    FarmMessage = 4
    TrapMessage = 5
    EconomyResourceMessage = 6
    AdditionResourceMessage = 7
    MapMessage = 8
    NewsMessage = 9
    TeamMessage = 10


class NewsType(Enum):
    NullNewsType = 0
    TextMessage = 1
    BinaryMessage = 2


class Character:
    """
    :attr guid: 玩家唯一标识符
    :attr teamID: 玩家所在队伍ID
    :attr playerID: 玩家ID
    :attr characterType: 玩家角色类型
    :attr characterActiveState: 玩家主动状态
    :attr isBlind: 是否失明
    :attr blindTime: 失明时间
    :attr isStunned: 是否眩晕
    :attr stunnedTime: 眩晕时间
    :attr isInvisible: 是否隐身
    :attr invisibleTime: 隐身时间
    :attr isBurned: 是否烧伤
    :attr burnedTime: 烧伤时间
    :attr harmCut: 伤害减免比例
    :attr harmCutTime: 伤害减免时间
    :attr characterPassiveState: 玩家最新被动状态
    :attr x: 玩家坐标x
    :attr y: 玩家坐标y
    :attr facingDirection: 玩家朝向
    :attr speed: 玩家速度
    :attr viewRange: 玩家视野范围
    :attr commonAttack: 普通攻击力
    :attr commonAttackCD: 普通攻击冷却时间
    :attr commonAttackRange: 普通攻击范围
    :attr skillAttackCD: 技能攻击冷却时间
    :attr economyDepletion: 经济消耗
    :attr killScore: 击杀得分
    :attr hp: 玩家血量
    :attr shieldEquipment: 装备护盾剩余值
    :attr shoesEquipment: 装备鞋子加成值
    :attr shoesTime: 鞋子加成时间
    :attr isPurified: 是否被净化
    :attr purifiedTime: 净化时间
    :attr isBerserk: 是否狂暴
    :attr berserkTime: 狂暴时间
    :attr attackBuffNum: 加成资源attackBuff等级
    :attr attackBuffTime: 加成资源attackBuff时间
    :attr speedBuffTime: 加成资源speedBuff时间
    :attr visionBuffTime: 加成资源visionBuff时间
    """

    def __init__(self):
        self.guid: int = 0
        self.teamID: int = 0
        self.playerID: int = 0
        self.characterType: CharacterType = CharacterType.NullCharacterType

        # 主动状态
        self.characterActiveState: CharacterState = CharacterState.NullCharacterState

        # 被动状态
        self.isBlind: bool = False
        self.blindTime: int = 0
        self.isStunned: bool = False
        self.stunnedTime: int = 0
        self.isInvisible: bool = False
        self.invisibleTime: int = 0
        self.isBurned: bool = False
        self.burnedTime: int = 0
        self.harmCut: float = 1.0
        self.harmCutTime: int = 0

        # 最新被动状态状态
        self.characterPassiveState: CharacterState = CharacterState.NullCharacterState

        # 坐标位置和朝向
        self.x: int = 0
        self.y: int = 0

        # 朝向和速度
        self.facingDirection: float = 0.0
        self.speed: int = 0
        self.viewRange: int = 0

        # 普通攻击相关属性
        self.commonAttack: int = 0
        self.commonAttackCD: int = 0
        self.commonAttackRange: int = 0

        # 技能攻击相关属性
        self.skillAttackCD: int = 0

        # 消耗资源相关属性
        self.economyDepletion: int = 0

        # 击杀得分相关属性
        self.killScore: int = 0

        # 血量相关属性
        self.hp: int = 0

        # 装备相关属性，装备护盾剩余值?
        self.shieldEquipment: int = 0
        self.shoesEquipment: int = 0
        self.shoesTime: int = 0
        self.isPurified: bool = False
        self.purifiedTime: int = 0
        self.isBerserk: bool = False
        self.berserkTime: int = 0

        # 加成资源的Buff
        self.attackBuffNum: int = 0
        self.attackBuffTime: int = 0
        self.speedBuffTime: int = 0
        self.visionBuffTime: int = 0


class Team:
    def __init__(self):
        self.playerID: int = 0
        self.teamID: int = 0
        self.score: int = 0
        self.energy: int = 0


class Home:
    def __init__(self):
        self.x: int = 0
        self.y: int = 0
        self.hp: int = 0
        self.teamID: int = 0
        self.guid: int = 0


class Trap:
    def __init__(self):
        self.trapType: TrapType = TrapType.NullTrapType
        self.x: int = 0
        self.y: int = 0
        self.teamID: int = 0
        self.id: int = 0


class EconomyResource:
    def __init__(self):
        self.economyResourceType: EconomyResourceType = (
            EconomyResourceType.NullEconomyResourceType
        )
        self.economyResourceState: EconomyResourceState = (
            EconomyResourceState.NullEconomyResourceState
        )
        self.x: int = 0
        self.y: int = 0
        self.process: int = 0
        self.id: int = 0


class AdditionResource:
    def __init__(self):
        self.additionResourceType: AdditionResourceType = (
            AdditionResourceType.NullAdditionReourceType
        )
        self.additionResourceState: AdditionResourceState = (
            AdditionResourceState.NullAdditionResourceState
        )
        self.x: int = 0
        self.y: int = 0
        self.hp: int = 0
        self.id: int = 0


class ConstructionState:
    def __init__(self, teamID, HP, type: ConstructionType):
        self.teamID = teamID
        self.hp = HP
        self.constructionType: ConstructionType = ConstructionType.NullConstructionType


class GameMap:
    """
    :attr barracksState: 兵营状态
    :attr springState: 泉水状态
    :attr farmState: 农场状态
    :attr trapState: 陷阱状态
    :attr economyResource: 经济资源状态
    :attr additionResource: 加成资源状态
    """

    def __init__(self):
        self.barracksState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.springState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.farmState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.trapState: Dict[Tuple[int, int], Tuple[int, int]] = {}

        # [x, y] -> process/hp
        self.economyResource: Dict[Tuple[int, int], int] = {}
        self.additionResource: Dict[Tuple[int, int], int] = {}


class GameInfo:
    def __init__(self):
        self.gameTime: int = 0
        self.buddhistsTeamScore: int = 0
        self.buddhistsTeamEconomy: int = 0
        self.buddhistsTeamHeroHp: int = 0
        self.monstersTeamScore: int = 0
        self.monstersTeamEconomy: int = 0
        self.monstersTeamHeroHp: int = 0
