from typing import Final, List

import proto.Message2Clients_pb2 as Message2Clients
import proto.Message2Server_pb2 as Message2Server
import proto.MessageType_pb2 as MessageType
import PyAPI.structures as THUAI8

numOfGridPerCell: Final[int] = 1000


class AssistFunction:
    @staticmethod
    def CellToGrid(cell: int) -> int:
        return cell * numOfGridPerCell + numOfGridPerCell // 2

    @staticmethod
    def GridToCell(grid: int) -> int:
        return grid // numOfGridPerCell

    @staticmethod
    def HaveView(
        viewRange: int,
        x: int,
        y: int,
        newX: int,
        newY: int,
        map: List[List[THUAI8.PlaceType]],
    ) -> bool:
        deltaX = newX - x
        deltaY = newY - y
        distance = deltaX**2 + deltaY**2
        myPlace = map[AssistFunction.GridToCell(x)][AssistFunction.GridToCell(y)]
        newPlace = map[AssistFunction.GridToCell(newX)][AssistFunction.GridToCell(newY)]
        if myPlace != THUAI8.PlaceType.Bush and newPlace == THUAI8.PlaceType.Bush:
            return False
        if distance > viewRange * viewRange:
            return False
        divide = max(abs(deltaX), abs(deltaY)) // 100
        if divide == 0:
            return True
        dx = deltaX / divide
        dy = deltaY / divide
        selfX = float(x)
        selfY = float(y)
        if newPlace == THUAI8.PlaceType.Bush and myPlace == THUAI8.PlaceType.Bush:
            for _ in range(divide):
                selfX += dx
                selfY += dy
                if (
                    map[AssistFunction.GridToCell(int(selfX))][
                        AssistFunction.GridToCell(int(selfY))
                    ]
                    != THUAI8.PlaceType.Bush
                ):
                    return False
            else:
                return True
        else:
            for _ in range(divide):
                selfX += dx
                selfY += dy
                if (
                    map[AssistFunction.GridToCell(int(selfX))][
                        AssistFunction.GridToCell(int(selfY))
                    ]
                    == THUAI8.PlaceType.Barrier
                ):
                    return False
            else:
                return True


class Proto2THUAI8:
    gameStateDict: Final[dict] = {
        MessageType.NULL_GAME_STATE: THUAI8.GameState.NullGameState,
        MessageType.GAME_START: THUAI8.GameState.GameStart,
        MessageType.GAME_RUNNING: THUAI8.GameState.GameRunning,
        MessageType.GAME_END: THUAI8.GameState.GameEnd,
    }

    placeTypeDict: Final[dict] = {
        MessageType.NULL_PLACE_TYPE: THUAI8.PlaceType.NullPlaceType,
        MessageType.HOME: THUAI8.PlaceType.Home,
        MessageType.SPACE: THUAI8.PlaceType.Space,
        MessageType.BARRIER: THUAI8.PlaceType.Barrier,
        MessageType.BUSH: THUAI8.PlaceType.Bush,
        MessageType.ECONOMY_RESOURCE: THUAI8.PlaceType.EconomyResource,
        MessageType.ADDITION_RESOURCE: THUAI8.PlaceType.AdditionResource,
        MessageType.CONSTRUCTION: THUAI8.PlaceType.Construction,
        MessageType.TRAP: THUAI8.PlaceType.Trap,
    }

    shapeTypeDict: Final[dict] = {
        MessageType.NULL_SHAPE_TYPE: THUAI8.ShapeType.NullShapeType,
        MessageType.CIRCLE: THUAI8.ShapeType.Circle,
        MessageType.SQUARE: THUAI8.ShapeType.Square,
    }

    playerTypeDict: Final[dict] = {
        MessageType.NULL_PLAYER_TYPE: THUAI8.PlayerType.NullPlayerType,
        MessageType.CHARACTER: THUAI8.PlayerType.Character,
        MessageType.TEAM: THUAI8.PlayerType.Team,
    }

    characterTypeDict: Final[dict] = {
        MessageType.NULL_CHARACTER_TYPE: THUAI8.CharacterType.NullCharacterType,
        MessageType.TangSeng: THUAI8.CharacterType.TangSeng,
        MessageType.SunWukong: THUAI8.CharacterType.SunWukong,
        MessageType.ZhuBajie: THUAI8.CharacterType.ZhuBajie,
        MessageType.ShaWujing: THUAI8.CharacterType.ShaWujing,
        MessageType.BaiLongma: THUAI8.CharacterType.BaiLongma,
        MessageType.Monkid: THUAI8.CharacterType.Monkid,
        MessageType.JiuLing: THUAI8.CharacterType.JiuLing,
        MessageType.HongHaier: THUAI8.CharacterType.HongHaier,
        MessageType.NiuMowang: THUAI8.CharacterType.NiuMowang,
        MessageType.TieShan: THUAI8.CharacterType.TieShan,
        MessageType.ZhiZhujing: THUAI8.CharacterType.ZhiZhujing,
        MessageType.Pawn: THUAI8.CharacterType.Pawn,
    }

    characterStateDict: Final[dict] = {
        MessageType.NULL_CHARACTER_STATE: THUAI8.CharacterState.NullCharacterState,
        MessageType.IDLE: THUAI8.CharacterState.Idle,
        MessageType.HARVESTING: THUAI8.CharacterState.Harvesting,
        MessageType.ATTACKING: THUAI8.CharacterState.Attacking,
        MessageType.SKILL_CASTING: THUAI8.CharacterState.SkillCasting,
        MessageType.CONSTRUCTING: THUAI8.CharacterState.Constructing,
        MessageType.MOVING: THUAI8.CharacterState.Moving,
        MessageType.BLIND: THUAI8.CharacterState.Blind,
        MessageType.KNOCKED_BACK: THUAI8.CharacterState.KnockedBack,
        MessageType.STUNNED: THUAI8.CharacterState.Stunned,
        MessageType.INVISIBLE: THUAI8.CharacterState.Invisible,
        MessageType.HEALING: THUAI8.CharacterState.Healing,
        MessageType.BERSERK: THUAI8.CharacterState.Berserk,
        MessageType.BURNED: THUAI8.CharacterState.Burned,
        MessageType.DECEASED: THUAI8.CharacterState.Deceased,
    }

    economyResourceTypeDict: Final[dict] = {
        MessageType.NULL_ECONOMY_RESOURCE_TYPE: THUAI8.EconomyResourceType.NullEconomyResourceType,
        MessageType.SMALL_ECONOMY_RESOURCE: THUAI8.EconomyResourceType.SmallEconomyResource,
        MessageType.MEDIUM_ECONOMY_RESOURCE: THUAI8.EconomyResourceType.MediumEconomyResource,
        MessageType.LARGE_ECONOMY_RESOURCE: THUAI8.EconomyResourceType.LargeEconomyResource,
    }

    additionResourceTypeDict: Final[dict] = {
        MessageType.NULL_ADDITION_RESOURCE_TYPE: THUAI8.AdditionResourceType.NullAdditionResourceType,
        MessageType.LIFE_POOL1: THUAI8.AdditionResourceType.LifePool1,
        MessageType.LIFE_POOL2: THUAI8.AdditionResourceType.LifePool2,
        MessageType.LIFE_POOL3: THUAI8.AdditionResourceType.LifePool3,
        MessageType.CRAZY_MAN1: THUAI8.AdditionResourceType.CrazyMan1,
        MessageType.CRAZY_MAN2: THUAI8.AdditionResourceType.CrazyMan2,
        MessageType.CRAZY_MAN3: THUAI8.AdditionResourceType.CrazyMan3,
        MessageType.QUICK_STEP: THUAI8.AdditionResourceType.QuickStep,
        MessageType.WIDE_VIEW: THUAI8.AdditionResourceType.WideView,
    }

    economyResourceStateTypeDict: Final[dict] = {
        MessageType.NULL_ECONOMY_RESOURCE_STSTE: THUAI8.EconomyResourceState.NullEconomyResourceState,
        MessageType.HARVESTABLE: THUAI8.EconomyResourceState.Harvestable,
        MessageType.BEING_HARVESTED: THUAI8.EconomyResourceState.BeingHarvested,
        MessageType.HARVESTED: THUAI8.EconomyResourceState.Harvested,
    }

    additionResourceStateTypeDict: Final[dict] = {
        MessageType.NULL_ADDITION_RESOURCE_STATE: THUAI8.AdditionResourceState.NullAdditionResourceState,
        MessageType.BEATABLE: THUAI8.AdditionResourceState.Beatable,
        MessageType.BEING_BEATEN: THUAI8.AdditionResourceState.BeingBeaten,
        MessageType.BEATEN: THUAI8.AdditionResourceState.Beaten,
    }

    equipmentTypeDict: Final[dict] = {
        MessageType.NULL_EQUIPMENT_TYPE: THUAI8.EquipmentType.NullEquipmentType,
        MessageType.SMALL_HEALTH_POTION: THUAI8.EquipmentType.SmallHealthPotion,
        MessageType.MEDIUM_HEALTH_POTION: THUAI8.EquipmentType.MediumHealthPotion,
        MessageType.LARGE_HEALTH_POTION: THUAI8.EquipmentType.LargeHealthPotion,
        MessageType.SMALL_SHIELD: THUAI8.EquipmentType.SmallShield,
        MessageType.MEDIUM_SHIELD: THUAI8.EquipmentType.MediumShield,
        MessageType.LARGE_SHIELD: THUAI8.EquipmentType.LargeShield,
        MessageType.SPEEDBOOTS: THUAI8.EquipmentType.Speedboots,
        MessageType.PURIFICATION_POTION: THUAI8.EquipmentType.PurificationPotion,
        MessageType.INVISIBILITY_POTION: THUAI8.EquipmentType.InvisibilityPotion,
        MessageType.BERSERK_POTION: THUAI8.EquipmentType.BerserkPotion,
    }

    constructionTypeDict: Final[dict] = {
        MessageType.NULL_CONSTRUCTION_TYPE: THUAI8.ConstructionType.NullConstructionType,
        MessageType.BARRACKS: THUAI8.ConstructionType.Barracks,
        MessageType.SPRING: THUAI8.ConstructionType.Spring,
        MessageType.FARM: THUAI8.ConstructionType.Farm,
    }

    trapTypeDict: Final[dict] = {
        MessageType.NULL_TRAP_TYPE: THUAI8.TrapType.NullTrapType,
        MessageType.HOLE: THUAI8.TrapType.Hole,
        MessageType.CAGE: THUAI8.TrapType.Cage,
    }

    newsTypeDict: Final[dict] = {
        MessageType.NULL_NEWS_TYPE: THUAI8.NewsType.NullNewsType,
        MessageType.TEXT: THUAI8.NewsType.TextMessage,
        MessageType.BINARY: THUAI8.NewsType.BinaryMessage,
    }

    @staticmethod
    def Protobuf2THUAI8Character(
        characterMsg: Message2Clients.MessageOfCharacter,
    ) -> THUAI8.Character:
        character = THUAI8.Character()
        character.guid = characterMsg.guid
        character.teamID = characterMsg.team_id
        character.playerID = characterMsg.player_id
        character.characterType = Proto2THUAI8.characterTypeDict[
            characterMsg.character_type
        ]
        character.characterActiveState = Proto2THUAI8.characterStateDict[
            characterMsg.character_active_state
        ]
        character.isBlind = characterMsg.is_blind
        character.blindTime = characterMsg.blind_time
        character.isStunned = characterMsg.is_stunned
        character.stunnedTime = characterMsg.stunned_time
        character.isInvisible = characterMsg.is_invisible
        character.invisibleTime = characterMsg.invisible_time
        character.isBurned = characterMsg.is_burned
        character.burnedTime = characterMsg.burned_time
        character.harmCut = characterMsg.harm_cut
        character.harmCutTime = characterMsg.harm_cut_time
        character.characterPassiveState = Proto2THUAI8.characterStateDict[
            characterMsg.character_passive_state
        ]
        character.x = characterMsg.x
        character.y = characterMsg.y
        character.facingDirection = characterMsg.facing_direction
        character.speed = characterMsg.speed
        character.viewRange = characterMsg.view_range
        character.commonAttack = characterMsg.common_attack
        character.commonAttackCD = characterMsg.common_attack_cd
        character.commonAttackRange = characterMsg.common_attack_range
        character.skillAttackCD = characterMsg.skill_attack_cd
        character.economyDepletion = characterMsg.economy_depletion
        character.killScore = characterMsg.kill_score
        character.hp = characterMsg.hp
        character.shieldEquipment = characterMsg.shield_equipment
        character.shoesEquipment = characterMsg.shoes_equipment
        character.shoesTime = characterMsg.shoes_time
        character.isPurified = characterMsg.is_purified
        character.purifiedTime = characterMsg.purified_time
        character.isBerserk = characterMsg.is_berserk
        character.berserkTime = characterMsg.berserk_time
        character.attackBuffNum = characterMsg.attack_buff_num
        character.attackBuffTime = characterMsg.attack_buff_time
        character.speedBuffTime = characterMsg.speed_buff_time
        character.visionBuffTime = characterMsg.vision_buff_time
        return character

    @staticmethod
    def Protobuf2THUAI8Team(
        teamMsg: Message2Clients.MessageOfTeam,
    ) -> THUAI8.Team:
        team = THUAI8.Team()
        team.teamID = teamMsg.team_id
        team.playerID = teamMsg.player_id
        team.score = teamMsg.score
        team.energy = teamMsg.energy
        return team

    @staticmethod
    def Protobuf2THUAI8GameInfo(
        gameInfoMsg: Message2Clients.MessageOfAll,
    ) -> THUAI8.GameInfo:
        gameInfo = THUAI8.GameInfo()
        gameInfo.gameSTime = gameInfoMsg.game_time
        gameInfo.buddhistsTeamEconomy = gameInfoMsg.buddhists_team_economy
        gameInfo.buddhistsTeamScore = gameInfoMsg.buddhists_team_score
        gameInfo.buddhistsTeamHeroHp = gameInfoMsg.buddhists_hero_hp
        gameInfo.monstersEconomy = gameInfoMsg.monsters_team_economy
        gameInfo.monstersTeamScore = gameInfoMsg.monsters_team_score
        gameInfo.monstersTeamHeroHp = gameInfoMsg.monsters_hero_hp
        return gameInfo

    @staticmethod
    def Protobuf2THUAI8Trap(
        trapMsg: Message2Clients.MessageOfTrap,
    ) -> THUAI8.Trap:
        trap = THUAI8.Trap()
        trap.trapType = Proto2THUAI8.trapTypeDict[trapMsg.trap_type]
        trap.x = trapMsg.x
        trap.y = trapMsg.y
        trap.teamID = trapMsg.team_id
        return trap

    @staticmethod
    def Protobuf2THUAI8EconomyResource(
        economyResourceMsg: Message2Clients.MessageOfEconomyResource,
    ) -> THUAI8.EconomyResource:
        economyResource = THUAI8.EconomyResource()
        economyResource.economyResourceType = Proto2THUAI8.economyResourceTypeDict[
            economyResourceMsg.economy_resource_type
        ]
        economyResource.economyResourceState = (
            Proto2THUAI8.economyResourceStateTypeDict[
                economyResourceMsg.economy_resource_state
            ]
        )
        economyResource.x = economyResourceMsg.x
        economyResource.y = economyResourceMsg.y
        return economyResource

    @staticmethod
    def Protobuf2THUAI8AdditionResource(
        additionResourceMsg: Message2Clients.MessageOfAdditionResource,
    ) -> THUAI8.AdditionResource:
        additionResource = THUAI8.AdditionResource()
        additionResource.additionResourceType = Proto2THUAI8.additionResourceTypeDict[
            additionResourceMsg.addition_resource_type
        ]
        additionResource.additionResourceState = (
            Proto2THUAI8.additionResourceStateTypeDict[
                additionResourceMsg.addition_resource_state
            ]
        )
        additionResource.x = additionResourceMsg.x
        additionResource.y = additionResourceMsg.y
        return additionResource

    # @staticmethod
    # def Protobuf2THUAI8ConstructionState(
    #     constructionStateMsg: Message2Clients.MessageOfConstructionState,
    # ) -> THUAI8.ConstructionState:
    #     constructionState = THUAI8.ConstructionState()
    #     constructionState.teamID = constructionStateMsg.team_id
    #     constructionState.hp = constructionStateMsg.hp
    #     constructionState.constructionType = Proto2THUAI8.constructionTypeDict[
    #         constructionStateMsg.construction_type
    #     ]
    #     return constructionState


class THUAI82Proto:
    gameStateDict: Final[dict] = {
        THUAI8.GameState.NullGameState: MessageType.NULL_GAME_STATE,
        THUAI8.GameState.GameStart: MessageType.GAME_START,
        THUAI8.GameState.GameRunning: MessageType.GAME_RUNNING,
        THUAI8.GameState.GameEnd: MessageType.GAME_END,
    }

    placeTypeDict: Final[dict] = {
        THUAI8.PlaceType.NullPlaceType: MessageType.NULL_PLACE_TYPE,
        THUAI8.PlaceType.Home: MessageType.HOME,
        THUAI8.PlaceType.Space: MessageType.SPACE,
        THUAI8.PlaceType.Barrier: MessageType.BARRIER,
        THUAI8.PlaceType.Bush: MessageType.BUSH,
        THUAI8.PlaceType.EconomyResource: MessageType.ECONOMY_RESOURCE,
        THUAI8.PlaceType.AdditionResource: MessageType.ADDITION_RESOURCE,
        THUAI8.PlaceType.Construction: MessageType.CONSTRUCTION,
        THUAI8.PlaceType.Trap: MessageType.TRAP,
    }

    shapeTypeDict: Final[dict] = {
        THUAI8.ShapeType.NullShapeType: MessageType.NULL_SHAPE_TYPE,
        THUAI8.ShapeType.Circle: MessageType.CIRCLE,
        THUAI8.ShapeType.Square: MessageType.SQUARE,
    }

    playerTeamDict: Final[dict] = {
        THUAI8.PlayerTeam.NullTeam: MessageType.NULL_TEAM,
        THUAI8.PlayerTeam.BuddhistsTeam: MessageType.BUDDHISTS_TEAM,
        THUAI8.PlayerTeam.MonstersTeam: MessageType.MONSTERS_TEAM,
    }

    characterTypeDict: Final[dict] = {
        THUAI8.CharacterType.NullCharacterType: MessageType.NULL_CHARACTER_TYPE,
        THUAI8.CharacterType.TangSeng: MessageType.TangSeng,
        THUAI8.CharacterType.SunWukong: MessageType.SunWukong,
        THUAI8.CharacterType.ZhuBajie: MessageType.ZhuBajie,
        THUAI8.CharacterType.ShaWujing: MessageType.ShaWujing,
        THUAI8.CharacterType.BaiLongma: MessageType.BaiLongma,
        THUAI8.CharacterType.Monkid: MessageType.Monkid,
        THUAI8.CharacterType.JiuLing: MessageType.JiuLing,
        THUAI8.CharacterType.HongHaier: MessageType.HongHaier,
        THUAI8.CharacterType.NiuMowang: MessageType.NiuMowang,
        THUAI8.CharacterType.TieShan: MessageType.TieShan,
        THUAI8.CharacterType.ZhiZhujing: MessageType.ZhiZhujing,
        THUAI8.CharacterType.Pawn: MessageType.Pawn,
    }

    equipmentTypeDict: Final[dict] = {
        THUAI8.EquipmentType.NullEquipmentType: MessageType.NULL_EQUIPMENT_TYPE,
        THUAI8.EquipmentType.SmallHealthPotion: MessageType.SMALL_HEALTH_POTION,
        THUAI8.EquipmentType.MediumHealthPotion: MessageType.MEDIUM_HEALTH_POTION,
        THUAI8.EquipmentType.LargeHealthPotion: MessageType.LARGE_HEALTH_POTION,
        THUAI8.EquipmentType.SmallShield: MessageType.SMALL_SHIELD,
        THUAI8.EquipmentType.MediumShield: MessageType.MEDIUM_SHIELD,
        THUAI8.EquipmentType.LargeShield: MessageType.LARGE_SHIELD,
        THUAI8.EquipmentType.Speedboots: MessageType.SPEEDBOOTS,
        THUAI8.EquipmentType.PurificationPotion: MessageType.PURIFICATION_POTION,
        THUAI8.EquipmentType.InvisibilityPotion: MessageType.INVISIBILITY_POTION,
        THUAI8.EquipmentType.BerserkPotion: MessageType.BERSERK_POTION,
    }

    characterStateDict: Final[dict] = {
        THUAI8.CharacterState.NullCharacterState: MessageType.NULL_CHARACTER_STATE,
        THUAI8.CharacterState.Idle: MessageType.IDLE,
        THUAI8.CharacterState.Harvesting: MessageType.HARVESTING,
        THUAI8.CharacterState.Attacking: MessageType.ATTACKING,
        THUAI8.CharacterState.SkillCasting: MessageType.SKILL_CASTING,
        THUAI8.CharacterState.Constructing: MessageType.CONSTRUCTING,
        THUAI8.CharacterState.Moving: MessageType.MOVING,
        THUAI8.CharacterState.Blind: MessageType.BLIND,
        THUAI8.CharacterState.KnockedBack: MessageType.KNOCKED_BACK,
        THUAI8.CharacterState.Stunned: MessageType.STUNNED,
        THUAI8.CharacterState.Invisible: MessageType.INVISIBLE,
        THUAI8.CharacterState.Healing: MessageType.HEALING,
        THUAI8.CharacterState.Berserk: MessageType.BERSERK,
        THUAI8.CharacterState.Burned: MessageType.BURNED,
    }

    economyResourceTypeDict: Final[dict] = {
        THUAI8.EconomyResourceType.NullEconomyResourceType: MessageType.NULL_ECONOMY_RESOURCE_TYPE,
        THUAI8.EconomyResourceType.SmallEconomyResource: MessageType.SMALL_ECONOMY_RESOURCE,
        THUAI8.EconomyResourceType.MediumEconomyResource: MessageType.MEDIUM_ECONOMY_RESOURCE,
        THUAI8.EconomyResourceType.LargeEconomyResource: MessageType.LARGE_ECONOMY_RESOURCE,
    }

    additionResourceTypeDict: Final[dict] = {
        THUAI8.AdditionResourceType.NullAdditionResourceType: MessageType.NULL_ADDITION_RESOURCE_TYPE,
        THUAI8.AdditionResourceType.LifePool1: MessageType.LIFE_POOL1,
        THUAI8.AdditionResourceType.LifePool2: MessageType.LIFE_POOL2,
        THUAI8.AdditionResourceType.LifePool3: MessageType.LIFE_POOL3,
        THUAI8.AdditionResourceType.CrazyMan1: MessageType.CRAZY_MAN1,
        THUAI8.AdditionResourceType.CrazyMan2: MessageType.CRAZY_MAN2,
        THUAI8.AdditionResourceType.CrazyMan3: MessageType.CRAZY_MAN3,
        THUAI8.AdditionResourceType.QuickStep: MessageType.QUICK_STEP,
        THUAI8.AdditionResourceType.WideView: MessageType.WIDE_VIEW,
    }

    economyResourceStateDict: Final[dict] = {
        THUAI8.EconomyResourceState.NullEconomyResourceState: MessageType.NULL_ECONOMY_RESOURCE_STSTE,
        THUAI8.EconomyResourceState.Harvestable: MessageType.HARVESTABLE,
        THUAI8.EconomyResourceState.BeingHarvested: MessageType.BEING_HARVESTED,
        THUAI8.EconomyResourceState.Harvested: MessageType.HARVESTED,
    }

    additionResourceStateDict: Final[dict] = {
        THUAI8.AdditionResourceState.NullAdditionResourceState: MessageType.NULL_ADDITION_RESOURCE_STATE,
        THUAI8.AdditionResourceState.Beatable: MessageType.BEATABLE,
        THUAI8.AdditionResourceState.BeingBeaten: MessageType.BEING_BEATEN,
        THUAI8.AdditionResourceState.Beaten: MessageType.BEATEN,
    }

    constructionTypeDict: Final[dict] = {
        THUAI8.ConstructionType.NullConstructionType: MessageType.NULL_CONSTRUCTION_TYPE,
        THUAI8.ConstructionType.Barracks: MessageType.BARRACKS,
        THUAI8.ConstructionType.Spring: MessageType.SPRING,
        THUAI8.ConstructionType.Farm: MessageType.FARM,
    }

    trapTypeDict: Final[dict] = {
        THUAI8.TrapType.NullTrapType: MessageType.NULL_TRAP_TYPE,
        THUAI8.TrapType.Hole: MessageType.HOLE,
        THUAI8.TrapType.Cage: MessageType.CAGE,
    }

    newsTypeDict: Final[dict] = {
        THUAI8.NewsType.NullNewsType: MessageType.NULL_NEWS_TYPE,
        THUAI8.NewsType.TextMessage: MessageType.TEXT,
        THUAI8.NewsType.BinaryMessage: MessageType.BINARY,
    }

    # messageOfObjDict: Final[dict] = {
    #     # THUAI8.MessageOfObj.NullMessageOfObj: MessageType.NULL_MESSAGE_OF_OBJ,
    #     THUAI8.MessageOfObj.CharacterMessage: MessageType.character_message,
    #     THUAI8.MessageOfObj.BarracksMessage: MessageType.barracks_message,
    #     THUAI8.MessageOfObj.SpringMessage: MessageType.spring_message,
    #     THUAI8.MessageOfObj.FarmMessage: MessageType.farm_message,
    #     THUAI8.MessageOfObj.TrapMessage: MessageType.trap_message,
    #     THUAI8.MessageOfObj.EconomyResourceMessage: MessageType.economy_resource_message,
    #     THUAI8.MessageOfObj.AdditionResourceMessage: MessageType.addition_resource_message,
    #     THUAI8.MessageOfObj.MapMessage: MessageType.map_message,
    #     THUAI8.MessageOfObj.TeamMessage: MessageType.team_message,
    #     THUAI8.MessageOfObj.NewsMessage: MessageType.news_message,
    # }

    @staticmethod
    def THUAI82ProtobufMoveMsg(
        team: int, character: int, time: int, angle: float
    ) -> Message2Server.MoveMsg:
        moveMsg = Message2Server.MoveMsg()
        moveMsg.character_id = character
        moveMsg.angle = angle
        moveMsg.time_in_milliseconds = time
        moveMsg.team_id = team
        return moveMsg

    @staticmethod
    def THUAI82ProtobufIDMsg(playerID: int, teamID: int) -> Message2Server.IDMsg:
        IDMsg = Message2Server.IDMsg()
        IDMsg.character_id = playerID
        IDMsg.team_id = teamID
        return IDMsg

    @staticmethod
    def THUAI82ProtobufEquipMsg(
        character_id: int, team_id: int, equipment_type: THUAI8.EquipmentType
    ) -> Message2Server.EquipMsg:
        equipMsg = Message2Server.EquipMsg()
        equipMsg.character_id = character_id
        equipMsg.team_id = team_id
        equipMsg.equipment_type = THUAI82Proto.equipmentTypeDict[equipment_type]
        return equipMsg

    @staticmethod
    def THUAI82ProtobufCreatCharacterMsg(
        team_id: int, character_type: THUAI8.CharacterType, birthpoint_index: int
    ) -> Message2Server.CreatCharacterMsg:
        creatCharacterMsg = Message2Server.CreatCharacterMsg()
        creatCharacterMsg.team_id = team_id
        creatCharacterMsg.character_type = THUAI82Proto.characterTypeDict[
            character_type
        ]
        creatCharacterMsg.birthpoint_index = birthpoint_index
        return creatCharacterMsg

    @staticmethod
    def THUAI82ProtobufConstructMsg(
        character_id: int, team_id: int, construction_type: THUAI8.ConstructionType
    ) -> Message2Server.ConstructMsg:
        constructMsg = Message2Server.ConstructMsg()
        constructMsg.character_id = character_id
        constructMsg.team_id = team_id
        constructMsg.construction_type = THUAI82Proto.constructionTypeDict[
            construction_type
        ]
        return constructMsg

    @staticmethod
    def THUAI82ProtobufCharacterMsg(
        character_id: int,
        team_id: int,
        character_type: THUAI8.CharacterType,
        side_flag: int,
    ) -> Message2Server.CharacterMsg:
        characterMsg = Message2Server.CharacterMsg()
        characterMsg.character_id = character_id
        characterMsg.team_id = team_id
        characterMsg.character_type = THUAI82Proto.characterTypeDict[character_type]
        characterMsg.side_flag = side_flag
        return characterMsg

    @staticmethod
    def THUAI82ProtobufCastMsg(
        character_id: int,
        skill_id: int,
        team_id: int,
        attack_range: int,
        x: int,
        y: int,
        angle: float,
    ) -> Message2Server.CastMsg:
        castMsg = Message2Server.CastMsg()
        castMsg.character_id = character_id
        castMsg.skill_id = skill_id
        castMsg.team_id = team_id
        castMsg.attack_range = attack_range
        castMsg.x = x
        castMsg.y = y
        castMsg.angle = angle
        return castMsg

    @staticmethod
    def THUAI82ProtobufAttackMsg(
        character_id: int, team_id: int, attacked_character_id: int, attack_range: int
    ) -> Message2Server.AttackMsg:
        attackMsg = Message2Server.AttackMsg()
        attackMsg.character_id = character_id
        attackMsg.team_id = team_id
        attackMsg.attacked_character_id = attacked_character_id
        attackMsg.attack_range = attack_range
        return attackMsg

    @staticmethod
    def THUAI82ProtobufAttackConstructionMsg(
        character_id: int, team_id: int
    ) -> Message2Server.AttackConstructionMsg:
        attackConstructionMsg = Message2Server.AttackConstructionMsg()
        attackConstructionMsg.character_id = character_id
        attackConstructionMsg.team_id = team_id
        return attackConstructionMsg

    @staticmethod
    def THUAI82ProtobufRecoverMsg(
        character_id: int, team_id: int, recover: int
    ) -> Message2Server.RecoverMsg:
        recoverMsg = Message2Server.RecoverMsg()
        recoverMsg.character_id = character_id
        recoverMsg.team_id = team_id
        recoverMsg.recover = recover
        return recoverMsg
