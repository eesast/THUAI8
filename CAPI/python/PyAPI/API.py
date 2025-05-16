import math
from concurrent.futures import Future, ThreadPoolExecutor
from typing import List, Tuple, Union, cast

import PyAPI.structures as THUAI8
from PyAPI.Interface import IAI, ICharacterAPI, IGameTimer, ILogic, ITeamAPI


class IAPI:
    @staticmethod
    def CellToGrid(cell: int) -> int:
        return cell * 1000 + 500

    @staticmethod
    def GridToCell(grid: int) -> int:
        return grid // 1000


class CharacterAPI(ICharacterAPI, IGameTimer):
    def __init__(self, logic: ILogic) -> None:
        self.__logic = logic
        self.__pool = ThreadPoolExecutor(20)

    # region 实现IGameTimer接口
    def StartTimer(self) -> None:
        pass

    def EndTimer(self) -> None:
        pass

    def Play(self, ai: IAI) -> None:
        ai.CharacterPlay(self)

    # endregion

    # region 实现IAPI接口
    # def SendTextMessage(self, toPlayerID: int, message: str) -> Future[bool]:
    #     return self.__pool.submit(self.__logic.Send, toPlayerID, message, False)

    # def SendBinaryMessage(self, toPlayerID: int, message: str) -> Future[bool]:
    #     return self.__pool.submit(self.__logic.Send, toPlayerID, message, True)

    def SendMessage(self, toPlayerID: int, message: Union[str, bytes]) -> Future[bool]:
        return self.__pool.submit(self.__logic.SendMessage, toPlayerID, message)

    def HaveMessage(self) -> bool:
        return self.__logic.HaveMessage()

    def GetMessage(self) -> Tuple[int, str]:
        return self.__logic.GetMessage()

    def GetFrameCount(self) -> int:
        return self.__logic.GetCounter()

    def Wait(self) -> bool:
        return self.__logic.WaitThread()

    def EndAllAction(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.EndAllAction)

    def GetCharacters(self) -> List[THUAI8.Character]:
        return self.__logic.GetCharacters()

    def GetEnemyCharacters(self) -> List[THUAI8.Character]:
        return self.__logic.GetEnemyCharacters()

    def GetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        return self.__logic.GetFullMap()

    def GetGameInfo(self) -> THUAI8.GameInfo:
        return self.__logic.GetGameInfo()

    def GetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        return self.__logic.GetPlaceType(cellX, cellY)

    def GetEconomyResourceState(self, cellX: int, cellY: int) -> THUAI8.EconomyResource:
        return self.__logic.GetEconomyResourceState(cellX, cellY)

    def GetAdditionResourceState(
        self, cellX: int, cellY: int
    ) -> THUAI8.AdditionResource:
        return self.__logic.GetAdditionResourceState(cellX, cellY)

    def GetConstructionState(self, cellX: int, cellY: int) -> THUAI8.ConstructionState:
        return self.__logic.GetConstructionState(cellX, cellY)

    def GetPlayerGUIDs(self) -> List[int]:
        return self.__logic.GetPlayerGUIDs()

    def GetEnergy(self) -> int:
        return self.__logic.GetEnergy()

    def GetScore(self) -> int:
        return self.__logic.GetScore()

    def Print(self, string: str) -> None:
        pass

    def PrintCharacter(self) -> None:
        pass

    def PrintTeam(self) -> None:
        pass

    def PrintSelfInfo(self) -> None:
        pass

    # endregion

    # region 实现ICharacterAPI接口
    def Move(self, timeInMilliseconds: int, angleInRadian: float) -> Future[bool]:
        return self.__pool.submit(self.__logic.Move, timeInMilliseconds, angleInRadian)

    def MoveRight(self, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(timeInMilliseconds, math.pi / 2)

    def MoveUp(self, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(timeInMilliseconds, math.pi)

    def MoveLeft(self, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(timeInMilliseconds, math.pi * 3 / 2)

    def MoveDown(self, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(timeInMilliseconds, 0)

    def Skill_Attack(self, angle: float) -> Future[bool]:
        return self.__pool.submit(self.__logic.Skill_Attack, angle)

    def Common_Attack(self, attackedPlayerID: int) -> Future[bool]:
        return self.__pool.submit(
            self.__logic.Common_Attack,
            attackedPlayerID,
        )

    def AttackConstruction(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.AttackConstruction)

    def AttackAdditionResource(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.AttackAdditionResource)

    def Recover(self, recover: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Recover, recover)

    # def Harvest(self) -> Future[bool]:
    #     return self.__pool.submit(self.__logic.Produce)

    def Rebuild(self, constructionType: THUAI8.ConstructionType) -> Future[bool]:
        return self.__pool.submit(self.__logic.Rebuild, constructionType)

    def Construct(self, constructionType: THUAI8.ConstructionType) -> Future[bool]:
        return self.__pool.submit(self.__logic.Construct, constructionType)

    def ConstructTrap(self, trapType: THUAI8.TrapType) -> Future[bool]:
        return self.__pool.submit(self.__logic.ConstructTrap, trapType)

    def Produce(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.Produce)

    def GetSelfInfo(self) -> THUAI8.Character:
        return cast(THUAI8.Character, self.__logic.GetSelfInfo())

    def HaveView(self, targetX: int, targetY: int) -> bool:
        self_info = self.GetSelfInfo()
        return self.__logic.HaveView(
            self_info.x, self_info.y, targetX, targetY, self_info.viewRange
        )

    # endregion


class TeamAPI(ITeamAPI, IGameTimer):
    def __init__(self, logic: ILogic) -> None:
        self.__logic = logic
        self.__pool = ThreadPoolExecutor(20)

    # region 实现IGameTimer接口
    def StartTimer(self) -> None:
        pass

    def EndTimer(self) -> None:
        pass

    def Play(self, ai: IAI) -> None:
        ai.TeamPlay(self)

    # endregion

    # region 实现IAPI接口
    # def SendTextMessage(self, toPlayerID: int, message: str) -> Future[bool]:
    #     return self.__pool.submit(self.__logic.Send, toPlayerID, message, False)

    # def SendBinaryMessage(self, toPlayerID: int, message: str) -> Future[bool]:
    #     return self.__pool.submit(self.__logic.Send, toPlayerID, message, True)

    def SendMessage(self, toPlayerID: int, message: Union[str, bytes]) -> Future[bool]:
        return self.__pool.submit(self.__logic.SendMessage, toPlayerID, message)

    def HaveMessage(self) -> bool:
        return self.__logic.HaveMessage()

    def GetMessage(self) -> Tuple[int, str]:
        return self.__logic.GetMessage()

    def GetFrameCount(self) -> int:
        return self.__logic.GetCounter()

    def Wait(self) -> bool:
        return self.__logic.WaitThread()

    def EndAllAction(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.EndAllAction)

    def GetCharacters(self) -> List[THUAI8.Character]:
        return self.__logic.GetCharacters()

    def GetEnemyCharacters(self) -> List[THUAI8.Character]:
        return self.__logic.GetEnemyCharacters()

    def GetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        return self.__logic.GetFullMap()

    def GetGameInfo(self) -> THUAI8.GameInfo:
        return self.__logic.GetGameInfo()

    def GetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        return self.__logic.GetPlaceType(cellX, cellY)

    def GetEconomyResourceState(self, cellX: int, cellY: int) -> THUAI8.EconomyResource:
        return self.__logic.GetEconomyResourceState(cellX, cellY)

    def GetAdditionResourceState(
        self, cellX: int, cellY: int
    ) -> THUAI8.AdditionResource:
        return self.__logic.GetAdditionResourceState(cellX, cellY)

    def GetConstructionState(self, cellX: int, cellY: int) -> THUAI8.ConstructionState:
        return self.__logic.GetConstructionState(cellX, cellY)

    def GetPlayerGUIDs(self) -> List[int]:
        return self.__logic.GetPlayerGUIDs()

    def GetEnergy(self) -> int:
        return self.__logic.GetEnergy()

    def GetScore(self) -> int:
        return self.__logic.GetScore()

    def Print(self, string: str) -> None:
        pass

    def PrintCharacter(self) -> None:
        pass

    def PrintTeam(self) -> None:
        pass

    def PrintSelfInfo(self) -> None:
        pass

    # endregion

    # region 实现ITeamAPI接口
    def GetSelfInfo(self) -> THUAI8.Team:
        return cast(THUAI8.Team, self.__logic.GetSelfInfo())

    def InstallEquipment(
        self, playerID: int, equipmentType: THUAI8.EquipmentType
    ) -> Future[bool]:
        return self.__pool.submit(
            self.__logic.InstallEquipment, playerID, equipmentType
        )

    def Recycle(self, playerID: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Recycle, playerID)

    def BuildCharacter(
        self, CharacterType: THUAI8.CharacterType, birthIndex: int
    ) -> Future[bool]:
        return self.__pool.submit(
            self.__logic.BuildCharacter, CharacterType, birthIndex
        )

    # endregion
