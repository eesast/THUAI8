import PyAPI.structures as THUAI8
import logging
import os
import datetime
from concurrent.futures import ThreadPoolExecutor, Future
from typing import List, Tuple, Optional, Dict, Any, Union
import math
from PyAPI.structures import PlaceType, ConstructionType, CharacterType, GameInfo, Character, ConstructionState, EconomyResourceState, AdditionResourceState
from PyAPI.Interface import ILogic, IAI

PI = math.pi

class CharacterDebugAPI:
    def __init__(self, logic: ILogic, file: bool, print: bool, warnOnly: bool, CharacterID: int):
        self.logic = logic
        self.playerID = CharacterID
        self.startPoint = datetime.datetime.now()
        self.__pool = ThreadPoolExecutor(20)
        self.logger = logging.getLogger(f"api {self.playerID}")
        self.logger.setLevel(logging.DEBUG)
        
        formatter = logging.Formattter(
            f"[api {self.playerID}] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
            "%H:%M:%S"
        )
        
        if not os.path.exists("logs"):
            os.makedirs("logs")
            
        fileHandler = logging.FileHandler(f"logs/api-{self.playerID}-log.txt", mode="w+", encoding="utf-8")
        printHandler = logging.StreamHandler()
        
        fileHandler.setFormatter(formatter)
        printHandler.setFormatter(formatter)
        
        fileHandler.setLevel(logging.TRACE if file else logging.OFF)
        print_level = logging.WARN if warnOnly else (logging.INFO if print else logging.OFF)
        printHandler.setLevel(print_level)
        
        self.logger.addHandler(fileHandler)
        self.logger.addHandler(printHandler)
        self.logger.propagate = False

    def StartTimer(self) -> None:
        self.startPoint = datetime.datetime.now()
        self.logger.info("=== AI.play() ===")
        self.logger.info(f"StartTimer: {self.startPoint.strftime('%H:%M:%S')}")

    def EndTimer(self) -> None:
        elapsed = (datetime.datetime.now() - self.startPoint).total_seconds() * 1000
        self.logger.info(f"Time elapsed: {elapsed:.2f}ms")

    def GetFrameCount(self) -> int:
        return self.logic.GetCounter()

    def SendTextMessage(self, toID: int, message: str) -> Future[bool]:
        self.logger.info(f"SendTextMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logSend, toID, message, False)

    def SendBinaryMessage(self, toID: int, message: bytes) -> Future[bool]:
        self.logger.info(f"SendBinaryMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logSend, toID, message, True)

    def __logSend(self, toID: int, message: Union[str, bytes], isBinary: bool) -> bool:
        result = self.logic.Send(toID, message, isBinary)
        if not result:
            self.logger.warning(f"Send failed at {self.__GetTime()}ms")
        return result

    def HaveMessage(self) -> bool:
        self.logger.info(f"HaveMessage: called at {self.__GetTime()}ms")
        result = self.logic.HaveMessage()
        if not result:
            self.logger.warning(f"HaveMessage failed at {self.__GetTime()}ms")
        return result

    def GetMessage(self) -> Tuple[int, str]:
        self.logger.info(f"GetMessage: called at {self.__GetTime()}ms")
        result = self.logic.GetMessage()
        if result[0] == -1:
            self.logger.warning(f"GetMessage failed at {self.__GetTime()}ms")
        return result

    def Wait(self) -> bool:
        self.logger.info(f"Wait: called at {self.__GetTime()}ms")
        return False if self.logic.GetCounter() == -1 else self.logic.WaitThread()

    def Move(self, timeInMilliseconds: int, angleInRadian: float) -> Future[bool]:
        self.logger.info(f"Move: time={timeInMilliseconds}ms, angle={angleInRadian}rad, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logMove, timeInMilliseconds, angleInRadian)

    def __logMove(self, time: int, angle: float) -> bool:
        result = self.logic.Move(time, angle)
        if not result:
            self.logger.warning(f"Move failed at {self.__GetTime()}ms")
        return result

    def MoveDown(self, time: int) -> Future[bool]:
        return self.Move(time, 0)

    def MoveRight(self, time: int) -> Future[bool]:
        return self.Move(time, PI * 0.5)

    def MoveUp(self, time: int) -> Future[bool]:
        return self.Move(time, PI)

    def MoveLeft(self, time: int) -> Future[bool]:
        return self.Move(time, PI * 1.5)

    def Skill_Attack(self, angleInRadian: float) -> Future[bool]:
        self.logger.info(f"Skill_Attack: angle={angleInRadian}rad, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logSkillAttack, angleInRadian)

    def __logSkillAttack(self, angle: float) -> bool:
        result = self.logic.SkillAttack(angle)
        if not result:
            self.logger.warning(f"Skill_Attack failed at {self.__GetTime()}ms")
        return result

    def Common_Attack(self, angleInRadian: float) -> Future[bool]:
        self.logger.info(f"Common_Attack: angle={angleInRadian}rad, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logCommonAttack, angleInRadian)

    def __logCommonAttack(self, angle: float) -> bool:
        result = self.logic.CommonAttack(angle)
        if not result:
            self.logger.warning(f"Common_Attack failed at {self.__GetTime()}ms")
        return result

    def Recover(self, recover: int) -> Future[bool]:
        self.logger.info(f"Recover: {recover}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logRecover, recover)

    def __logRecover(self, recover: int) -> bool:
        result = self.logic.Recover(recover)
        if not result:
            self.logger.warning(f"Recover failed at {self.__GetTime()}ms")
        return result

    def Harvest(self) -> Future[bool]:
        self.logger.info(f"Harvest: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logHarvest)

    def __logHarvest(self) -> bool:
        result = self.logic.Harvest()
        if not result:
            self.logger.warning(f"Harvest failed at {self.__GetTime()}ms")
        return result

    def Rebuild(self, constructionType: ConstructionType) -> Future[bool]:
        self.logger.info(f"Rebuild: {constructionType}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logRebuild, constructionType)

    def __logRebuild(self, ct: ConstructionType) -> bool:
        result = self.logic.Rebuild(ct)
        if not result:
            self.logger.warning(f"Rebuild failed at {self.__GetTime()}ms")
        return result

    def Construct(self, constructionType: ConstructionType) -> Future[bool]:
        self.logger.info(f"Construct: {constructionType}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logConstruct, constructionType)

    def __logConstruct(self, ct: ConstructionType) -> bool:
        result = self.logic.Construct(ct)
        if not result:
            self.logger.warning(f"Construct failed at {self.__GetTime()}ms")
        return result

    def GetCharacters(self) -> List[Character]:
        self.logger.info(f"GetCharacters: called at {self.__GetTime()}ms")
        result = self.logic.GetCharacters()
        if not result:
            self.logger.warning(f"GetCharacters failed at {self.__GetTime()}ms")
        return result

    def GetEnemyCharacters(self) -> List[Character]:
        self.logger.info(f"GetEnemyCharacters: called at {self.__GetTime()}ms")
        result = self.logic.GetEnemyCharacters()
        if not result:
            self.logger.warning(f"GetEnemyCharacters failed at {self.__GetTime()}ms")
        return result

    def GetFullMap(self) -> List[List[PlaceType]]:
        self.logger.info(f"GetFullMap: called at {self.__GetTime()}ms")
        result = self.logic.GetFullMap()
        if not result:
            self.logger.warning(f"GetFullMap failed at {self.__GetTime()}ms")
        return result

    def GetGameInfo(self) -> GameInfo:
        self.logger.info(f"GetGameInfo: called at {self.__GetTime()}ms")
        result = self.logic.GetGameInfo()
        if not result:
            self.logger.warning(f"GetGameInfo failed at {self.__GetTime()}ms")
        return result

    def GetPlaceType(self, cellX: int, cellY: int) -> Optional[PlaceType]:
        self.logger.info(f"GetPlaceType: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms")
        result = self.logic.GetPlaceType(cellX, cellY)
        if not result:
            self.logger.warning(f"GetPlaceType failed at {self.__GetTime()}ms")
        return result

    def GetEnconomyResourceState(self, cellX: int, cellY: int) -> Optional[EconomyResourceState]:
        self.logger.info(f"GetEnconomyResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms")
        result = self.logic.GetEnconomyResourceState(cellX, cellY)
        if not result:
            self.logger.warning(f"GetEnconomyResourceState failed at {self.__GetTime()}ms")
        return result

    def GetAdditionResourceState(self, cellX: int, cellY: int) -> Optional[AdditionResourceState]:
        self.logger.info(f"GetAdditionResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms")
        result = self.logic.GetAdditionResourceState(cellX, cellY)
        if not result:
            self.logger.warning(f"GetAdditionResourceState failed at {self.__GetTime()}ms")
        return result

    def GetConstructionState(self, cellX: int, cellY: int) -> Optional[ConstructionState]:
        self.logger.info(f"GetConstructionState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms")
        result = self.logic.GetConstructionState(cellX, cellY)
        if not result:
            self.logger.warning(f"GetConstructionState failed at {self.__GetTime()}ms")
        return result

    def GetPlayerGUIDs(self) -> List[int]:
        self.logger.info(f"GetPlayerGUIDs: called at {self.__GetTime()}ms")
        result = self.logic.GetPlayerGUIDs()
        if not result:
            self.logger.warning(f"GetPlayerGUIDs failed at {self.__GetTime()}ms")
        return result

    def GetEnergy(self) -> int:
        self.logger.info(f"GetEnergy: called at {self.__GetTime()}ms")
        result = self.logic.GetEnergy()
        if result == -1:
            self.logger.warning(f"GetEnergy failed at {self.__GetTime()}ms")
        return result

    def GetScore(self) -> int:
        self.logger.info(f"GetScore: called at {self.__GetTime()}ms")
        result = self.logic.GetScore()
        if result == -1:
            self.logger.warning(f"GetScore failed at {self.__GetTime()}ms")
        return result

    def GetSelfInfo(self) -> Character:
        self.logger.info(f"GetSelfInfo: called at {self.__GetTime()}ms")
        result = self.logic.GetSelfInfo()
        if not result:
            self.logger.warning(f"GetSelfInfo failed at {self.__GetTime()}ms")
        return result

    def Print(self, string: str) -> None:
        self.logger.info(string)

    def PrintCharacter(self) -> None:
        for char in self.logic.GetCharacters():
            self.logger.info("******Character Info******")
            self.logger.info(f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}")
            self.logger.info(f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}")
            self.logger.info("**************************")

    def PrintSelfInfo(self) -> None:
        selfInfo = self.GetSelfInfo()
        self.logger.info("******Self Info******")
        self.logger.info(f"type={selfInfo.characterType}, ID={selfInfo.characterID}, GUID={selfInfo.guid}")
        self.logger.info(f"x={selfInfo.x}, y={selfInfo.y}, state={selfInfo.characterState}")
        self.logger.info("*********************")

    def EndAllAction(self) -> Future[bool]:
        self.logger.info(f"EndAllAction: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.logic.EndAllAction)

    def __GetTime(self) -> float:
        return (datetime.datetime.now() - self.startPoint).total_seconds() * 1000

    def Play(self, ai: IAI) -> None:
        ai.play(self)

class TeamDebugAPI(CharacterDebugAPI):
    def __init__(self, logic: ILogic, file: bool, print: bool, warnOnly: bool, playerID: int):
        super().__init__(logic, file, print, warnOnly, playerID)
        # 覆盖父类logger配置
        self.logger.handlers.clear()
        formatter = logging.Formatter(
            f"[api{self.playerID}] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
            "%H:%M:%S"
        )
        fileHandler = logging.FileHandler(f"logs/api-{self.playerID}-log.txt", mode="w+", encoding="utf-8")
        printHandler = logging.StreamHandler()
        fileHandler.setFormatter(formatter)
        printHandler.setFormatter(formatter)
        fileHandler.setLevel(logging.TRACE if file else logging.OFF)
        print_level = logging.WARN if warnOnly else (logging.INFO if print else logging.OFF)
        printHandler.setLevel(print_level)
        self.logger.addHandler(fileHandler)
        self.logger.addHandler(printHandler)

    def InstallEquipment(self, playerID: int, equipmentType: Any) -> Future[bool]:
        self.logger.info(f"InstallEquipment: playerID={playerID}, type={equipmentType}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logInstall, playerID, equipmentType)

    def __logInstall(self, pid: int, et: Any) -> bool:
        result = self.logic.InstallEquipment(pid, et)
        if not result:
            self.logger.warning(f"InstallEquipment failed at {self.__GetTime()}ms")
        return result

    def BuildCharacter(self, characterType: CharacterType, birthIndex: int) -> Future[bool]:
        self.logger.info(f"BuildCharacter: type={characterType}, index={birthIndex}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logBuild, characterType, birthIndex)

    def __logBuild(self, ct: CharacterType, bi: int) -> bool:
        result = self.logic.BuildCharacter(ct, bi)
        if not result:
            self.logger.warning(f"BuildCharacter failed at {self.__GetTime()}ms")
        return result

    def PrintSelfInfo(self) -> None:
        selfInfo = self.logic.GetSelfInfo()
        self.logger.info("******Team Info******")
        self.logger.info(f"teamID={selfInfo.teamID}, playerID={selfInfo.playerID}")
        self.logger.info(f"score={selfInfo.score}, energy={selfInfo.energy}")
        self.logger.info("*********************")