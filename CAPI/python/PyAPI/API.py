from PyAPI.structures import THUAI8
from PyAPI.Interface import ILogic, IAI, IGameTimer, ICharacterAPI, ITeamAPI
from concurrent.futures import ThreadPoolExecutor, Future
from typing import List, Optional, Tuple, cast, Union
import math


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
        ai.Play(self)
    # endregion

    # region 实现IAPI接口
    def SendTextMessage(self, toPlayerID: int, message: str) -> Future[bool]:
        return self.__pool.submit(self.__logic.Send, toPlayerID, message, False)

    def SendBinaryMessage(self, toPlayerID: int, message: str) -> Future[bool]:
        return self.__pool.submit(self.__logic.Send, toPlayerID, message, True)

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

    def GetEnconomyResourceState(self, cellX: int, cellY: int) -> Optional[THUAI8.EconomyResourceState]:
        return self.__logic.GetEnconomyResourceState(cellX, cellY)

    def GetAdditionResourceState(self, cellX: int, cellY: int) -> Optional[THUAI8.AdditionResourceState]:
        return self.__logic.GetAdditionResourceState(cellX, cellY)

    def GetConstructionState(self, cellX: int, cellY: int) -> Optional[THUAI8.ConstructionState]:
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
    def Move(self, speed: int, timeInMilliseconds: int, angleInRadian: float) -> Future[bool]:
        return self.__pool.submit(self.__logic.Move, speed, timeInMilliseconds, angleInRadian)

    def MoveRight(self, speed: int, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(speed, timeInMilliseconds, math.pi/2)

    def MoveUp(self, speed: int, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(speed, timeInMilliseconds, math.pi)

    def MoveLeft(self, speed: int, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(speed, timeInMilliseconds, math.pi*3/2)

    def MoveDown(self, speed: int, timeInMilliseconds: int) -> Future[bool]:
        return self.Move(speed, timeInMilliseconds, 0)

    def Skill_Attack(self, attackedPlayerID: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Skill_Attack, attackedPlayerID)

    def Common_Attack(self, attackedPlayerID: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Common_Attack, attackedPlayerID)

    def Recover(self, recover: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Recover, recover)

    def Harvest(self) -> Future[bool]:
        return self.__pool.submit(self.__logic.Produce)

    def Rebuild(self, constructionType: THUAI8.ConstructionType) -> Future[bool]:
        return self.__pool.submit(self.__logic.Rebuild, constructionType)

    def Construct(self, constructionType: THUAI8.ConstructionType) -> Future[bool]:
        return self.__pool.submit(self.__logic.Construct, constructionType)

    def GetSelfInfo(self) -> THUAI8.Character:
        return cast(THUAI8.Character, self.__logic.CharacterGetSelfInfo())

    def HaveView(self, targetX: int, targetY: int) -> bool:
        self_info = self.GetSelfInfo()
        return self.__logic.HaveView(
            self_info.x, self_info.y, 
            targetX, targetY,
            self_info.viewRange
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
        ai.Play(self)
    # endregion

    # region 实现IAPI接口
    def SendTextMessage(self, toPlayerID: int, message: str) -> Future[bool]:
        return self.__pool.submit(self.__logic.Send, toPlayerID, message, False)

    def SendBinaryMessage(self, toPlayerID: int, message: str) -> Future[bool]:
        return self.__pool.submit(self.__logic.Send, toPlayerID, message, True)

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

    def GetEnconomyResourceState(self, cellX: int, cellY: int) -> Optional[THUAI8.EconomyResourceState]:
        return self.__logic.GetEnconomyResourceState(cellX, cellY)

    def GetAdditionResourceState(self, cellX: int, cellY: int) -> Optional[THUAI8.AdditionResourceState]:
        return self.__logic.GetAdditionResourceState(cellX, cellY)

    def GetConstructionState(self, cellX: int, cellY: int) -> Optional[THUAI8.ConstructionState]:
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
        return cast(THUAI8.Team, self.__logic.TeamGetSelfInfo())

    def InstallEquipment(self, playerID: int, equipmentType: THUAI8.EquipmentType) -> Future[bool]:
        return self.__pool.submit(self.__logic.InstallEquipment, playerID, equipmentType)

    def Recycle(self, playerID: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.Recycle, playerID)

    def BuildCharacter(self, CharacterType: THUAI8.CharacterType, birthIndex: int) -> Future[bool]:
        return self.__pool.submit(self.__logic.BuildCharacter, CharacterType, birthIndex)
    # endregion