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
    Camp1Character1 = 1
    Camp1Character2 = 2
    Camp1Character3 = 3
    Camp1Character4 = 4
    Camp1Character5 = 5
    Camp1Character6 = 6

    Camp2Character1 = 7
    Camp2Character2 = 8
    Camp2Character3 = 9
    Camp2Character4 = 10
    Camp2Character5 = 11
    Camp2Character6 = 12


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
    NullAdditionReourceType = 0

    SmallAdditionResource1 = 1
    MediumAdditionResource1 = 2
    LargeAdditionResource1 = 3

    SmallAdditionResource2 = 4
    MediumAdditionResource2 = 5
    LargeAdditionResource2 = 6

    AdditionResource3 = 7

    AdditionResource4 = 8


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
    def __init__(self):
        self.guid: int = 0
        self.teamID: int = 0
        self.playerID: int = 0
        self.characterType: CharacterType = CharacterType.NullChatacterType
        self.characterActiveState: CharacterState = CharacterState.NullCharacterState
        self.blindState: CharacterState = CharacterState.NullCharacterState
        self.blindTime: int = 0
        self.stunnedState: CharacterState = CharacterState.NullCharacterState
        self.stunnedTime: int = 0
        self.invisibleState: CharacterState = CharacterState.NullCharacterState
        self.invisibleTime: int = 0
        self.burnedState: CharacterState = CharacterState.NullCharacterState
        self.burnedTime: int = 0
        self.harmCut: float = 0.0
        self.harmCutTime: int = 0
        self.deceasedState: CharacterState = CharacterState.NullCharacterState
        self.characterPassiveState: CharacterState = CharacterState.NullCharacterState
        self.x: int = 0
        self.y: int = 0
        self.facingDirection: float = 0.0
        self.speed: int = 0
        self.viewRange: int = 0
        self.commonAttack: int = 0
        self.commonAttackCD: int = 0
        self.commonAttackRange: int = 0
        self.skillAttackCD: int = 0
        self.economyDepletion: int = 0
        self.killScore: int = 0
        self.hp: int = 0
        self.shieldEquipment: int = 0
        self.shoesEquipment: int = 0
        self.shoesEquipmentTime: int = 0
        self.purificationEquipmentTime: int = 0
        self.attackBuffTime: int = 0
        self.speedBuffTime: int = 0
        self.visionBuffTime: int = 0


class Team:
    def __init__(self):
        self.playerID: int = 0
        self.teamID: int = 0
        self.score: int = 0
        self.energy: int = 0


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


class ConstructionState:
    def __init__(self, teamID, HP, type: ConstructionType):
        self.teamID = teamID
        self.hp = HP
        self.constructionType = type


class GameMap:
    def __init__(self):
        self.barracksState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.springState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.farmState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.trapState: Dict[Tuple[int, int], Tuple[int, int]] = {}
        self.economyResource: Dict[Tuple[int, int], int] = {}
        self.additionResource: Dict[Tuple[int, int], int] = {}


class GameInfo:
    """
    :attr gameTime: 当前游戏时间
    :attr redScore: 红队当前分数
    :attr redEnergy: 红队当前经济
    :attr redHomeHp: 红队当前基地血量
    :attr blueScore: 蓝队当前分数
    :attr blueEnergy: 蓝队当前经济
    :attr blueHomeHp: 蓝队当前基地血量
    """

    def __init__(self):
        self.gameTime: int = 0
        self.buddhistsTeamScore: int = 0
        self.buddhistsTeamEconomy: int = 0
        self.buddhistsTeamHeroHp: int = 0
        self.monstersScore: int = 0
        self.monstersEconomy: int = 0
        self.monstersHeroHp: int = 0
