import datetime
import logging
import os
from concurrent.futures import Future, ThreadPoolExecutor
from math import pi as PI
from typing import List, Tuple, Union, cast

import PyAPI.structures as THUAI8
from PyAPI.Interface import IAI, ICharacterAPI, IGameTimer, ILogic, ITeamAPI


class CharacterDebugAPI(ICharacterAPI, IGameTimer):
    def __init__(
        self,
        logic: ILogic,
        file: bool,
        screen: bool,
        warnOnly: bool,
        playerID: int,
        teamID: int,
    ) -> None:
        self.__logic = logic
        self.__pool = ThreadPoolExecutor(20)
        self.__startPoint = datetime.datetime.now()
        self.__logger = logging.getLogger("api " + str(playerID))
        self.__logger.setLevel(logging.DEBUG)
        formatter = logging.Formatter(
            "[%(name)s] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
            "%H:%M:%S",
        )
        # 确保文件存在
        if not os.path.exists(
            os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
        ):
            os.makedirs(
                os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
            )

        fileHandler = logging.FileHandler(
            os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
            + f"/logs/api-{teamID}-{playerID}-log.txt",
            mode="w+",
            encoding="utf-8",
        )
        screenHandler = logging.StreamHandler()
        if file:
            fileHandler.setLevel(logging.DEBUG)
            fileHandler.setFormatter(formatter)
            self.__logger.addHandler(fileHandler)
        if screen:
            if warnOnly:
                screenHandler.setLevel(logging.WARNING)
            else:
                screenHandler.setLevel(logging.INFO)
            screenHandler.setFormatter(formatter)
            self.__logger.addHandler(screenHandler)

    def Move(
        self, speed: int, timeInMilliseconds: int, angleInRadian: float
    ) -> Future[bool]:
        self.__logger.info(
            f"Move: speed={speed}, time={timeInMilliseconds}ms, angle={angleInRadian}rad, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logMove)

    def __logMove(self, speed: int, time: int, angle: float) -> bool:
        result = self.__logic.Move(speed, time, angle)
        if not result:
            self.__logger.warning(f"Move failed at {self.__GetTime()}ms")
        return result

    def MoveDown(self, time: int, speed: int) -> Future[bool]:
        self.__logger.info(
            f"MoveDown: time={time}ms, speed={speed}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logMoveDown, time, speed, 0)

    def __logMoveDown(self, time: int, speed: int) -> bool:
        result = self.__logic.Move(time, speed, 0)
        if not result:
            self.__logger.warning(f"MoveDown failed at {self.__GetTime()}ms")
        return result

    def MoveRight(self, time: int, speed: int) -> Future[bool]:
        self.__logger.info(
            f"MoveRight: time={time}ms, speed={speed}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logMoveRight, time, speed, PI * 0.5)

    def __logMoveRight(self, time: int, speed: int) -> bool:
        result = self.__logic.Move(time, speed, PI * 0.5)
        if not result:
            self.__logger.warning(f"MoveRight failed at {self.__GetTime()}ms")
        return result

    def MoveUp(self, time: int, speed: int) -> Future[bool]:
        self.__logger.info(
            f"MoveUp: time={time}ms, speed={speed}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logMoveUp, time, speed, PI)

    def __logMoveUp(self, time: int, speed: int) -> bool:
        result = self.__logic.Move(time, speed, PI)
        if not result:
            self.__logger.warning(f"MoveUp failed at {self.__GetTime()}ms")
        return result

    def MoveLeft(self, time: int, speed: int) -> Future[bool]:
        self.__logger.info(
            f"MoveLeft: time={time}ms, speed={speed}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logMoveLeft, time, speed, PI * 1.5)

    def __logMoveLeft(self, time: int, speed: int) -> bool:
        result = self.__logic.Move(time, speed, PI * 1.5)
        if not result:
            self.__logger.warning(f"MoveLeft failed at {self.__GetTime()}ms")
        return result

    def Skill_Attack(self, angle: float) -> Future[bool]:
        self.__logger.info(
            f"Skill_Attack: angle={angle}rad, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logSkillAttack, angle)

    def __logSkillAttack(self, angle: float) -> bool:
        result = self.__logic.SkillAttack(angle)
        if not result:
            self.__logger.warning(f"Skill_Attack failed at {self.__GetTime()}ms")
        return result

    def Common_Attack(self, attackedPlayerID: int, attackedTeamID: int) -> Future[bool]:
        self.__logger.info(
            f"Common_Attack: attackedPlayerID={attackedPlayerID}, attackedTeamID={attackedTeamID}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(
            self.__logCommonAttack, attackedPlayerID, attackedTeamID
        )

    def __logCommonAttack(self, attackedPlayerID: int, attackedTeamID: int) -> bool:
        result = self.__logic.CommonAttack(attackedPlayerID, attackedTeamID)
        if not result:
            self.__logger.warning(f"Common_Attack failed at {self.__GetTime()}ms")
        return result

    def Recover(self, recover: int) -> Future[bool]:
        self.__logger.info(f"Recover: {recover}, called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logRecover, recover)

    def __logRecover(self, recover: int) -> bool:
        result = self.__logic.Recover(recover)
        if not result:
            self.__logger.warning(f"Recover failed at {self.__GetTime()}ms")
        return result

    def Produce(self):
        self.__logger.info(f"Harvest: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logProduce)

    def __logProduce(self) -> bool:
        result = self.__logic.Produce()
        if not result:
            self.__logger.warning(f"Harvest failed at {self.__GetTime()}ms")
        return result

    def Construct(self, constructionType):
        self.__logger.info(
            f"Construct: {constructionType}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logConstruct, constructionType)

    def __logConstruct(self, constructionType) -> bool:
        result = self.__logic.Construct(constructionType)
        if not result:
            self.__logger.warning(f"Construct failed at {self.__GetTime()}ms")
        return result

    def Rebuild(self, constructionType):
        self.__logger.info(
            f"Rebuild: {constructionType}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logRebuild, constructionType)

    def __logRebuild(self, constructionType) -> bool:
        result = self.__logic.Rebuild(constructionType)
        if not result:
            self.__logger.warning(f"Rebuild failed at {self.__GetTime()}ms")
        return result

    def Wait(self) -> bool:
        self.__logger.info(f"Wait: called at {self.__GetTime()}ms")
        if self.__logic.GetCounter() == -1:
            return False
        else:
            return self.__logic.WaitThread()

    def EndAllAction(self) -> Future[bool]:
        self.__logger.info(f"EndAllAction: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logEnd)

    def __logEnd(self) -> bool:
        result = self.__logic.EndAllAction()
        if not result:
            self.__logger.warning(f"EndAllAction failed at {self.__GetTime()}ms")
        return result

    def SendMessage(self, toID: int, message: Union[str, bytes]) -> Future[bool]:
        self.__logger.info(
            f"SendMessage: toID = {toID}, message = {message}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logSend, toID, message)

    def __logSend(self, toID: int, message: Union[str, bytes]) -> bool:
        result = self.__logic.Send(toID, message)
        if not result:
            self.__logger.warning(f"SendMessage failed at {self.__GetTime()}ms")
        return result

    def HaveMessage(self) -> bool:
        self.__logger.info(f"HaveMessage: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logHaveMessage)

    def __logHaveMessage(self) -> bool:
        result = self.__logic.HaveMessage()
        if not result:
            self.__logger.warning(f"HaveMessage failed at {self.__GetTime()}ms")
        return result

    def GetMessage(self) -> Tuple[int, Union[str, bytes]]:
        self.__logger.info(f"GetMessage: called at {self.__GetTime()}ms")
        result = self.__logic.GetMessage()
        if result[0] == -1:
            self.__logger.warning(f"GetMessage: failed at {self.__GetTime()}ms")
        return result

    def GetFrameCount(self) -> int:
        return self.__logic.GetFrameCount()

    def GetCharacters(self) -> List[THUAI8.Character]:
        self.__logger.info(f"GetCharacters: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetCharacters)

    def __logGetCharacters(self) -> List[THUAI8.Character]:
        result = self.__logic.GetCharacters()
        if not result:
            self.__logger.warning(f"GetCharacters failed at {self.__GetTime()}ms")
        return result

    def GetEnemyCharacters(self) -> List[THUAI8.Character]:
        self.__logger.info(f"GetEnemyCharacters: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetEnemyCharacters)

    def __logGetEnemyCharacters(self) -> List[THUAI8.Character]:
        result = self.__logic.GetEnemyCharacters()
        if not result:
            self.__logger.warning(f"GetEnemyCharacters failed at {self.__GetTime()}ms")
        return result

    def GetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        self.__logger.info(f"GetFullMap: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetFullMap)

    def __logGetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        result = self.__logic.GetFullMap()
        if not result:
            self.__logger.warning(f"GetFullMap failed at {self.__GetTime()}ms")
        return result

    def GetGameInfo(self) -> THUAI8.GameInfo:
        self.__logger.info(f"GetGameInfo: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetGameInfo)

    def __logGetGameInfo(self) -> THUAI8.GameInfo:
        result = self.__logic.GetGameInfo()
        if not result:
            self.__logger.warning(f"GetGameInfo failed at {self.__GetTime()}ms")
        return result

    def GetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        self.__logger.info(
            f"GetPlaceType: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetPlaceType, cellX, cellY)

    def __logGetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        result = self.__logic.GetPlaceType(cellX, cellY)
        if not result:
            self.__logger.warning(f"GetPlaceType failed at {self.__GetTime()}ms")
        return result

    def GetEconomyResourceState(self, cellX: int, cellY: int) -> int:
        self.__logger.info(
            f"GetEconomyResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetEconomyResourceState, cellX, cellY)

    def __logGetEconomyResourceState(self, cellX: int, cellY: int) -> int:
        result = self.__logic.GetEconomyResourceState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetEconomyResourceState failed at {self.__GetTime()}ms"
            )
        return result

    def GetAdditionResourceState(self, cellX: int, cellY: int) -> int:
        self.__logger.info(
            f"GetAdditionResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetAdditionResourceState, cellX, cellY)

    def __logGetAdditionResourceState(self, cellX: int, cellY: int) -> int:
        result = self.__logic.GetAdditionResourceState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetAdditionResourceState failed at {self.__GetTime()}ms"
            )
        return result

    def GetConstructionState(self, cellX, cellY) -> int:
        self.__logger.info(
            f"GetConstructionState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetConstructionState, cellX, cellY)

    def __logGetConstructionState(self, cellX, cellY) -> int:
        result = self.__logic.GetConstructionState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetConstructionState failed at {self.__GetTime()}ms"
            )
        return result

    def GetPlayerGUIDs(self):
        self.__logger.info(f"GetPlayerGUIDs: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetPlayerGUIDs)

    def __logGetPlayerGUIDs(self):
        result = self.__logic.GetPlayerGUIDs()
        if not result:
            self.__logger.warning(f"GetPlayerGUIDs failed at {self.__GetTime()}ms")
        return result

    def GetEnergy(self) -> int:
        self.__logger.info(f"GetEnergy: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetEnergy)

    def __logGetEnergy(self) -> int:
        result = self.__logic.GetEnergy()
        if result == -1:
            self.__logger.warning(f"GetEnergy failed at {self.__GetTime()}ms")
        return result

    def GetScore(self) -> int:
        self.__logger.info(f"GetScore: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetScore)

    def __logGetScore(self) -> int:
        result = self.__logic.GetScore()
        if result == -1:
            self.__logger.warning(f"GetScore failed at {self.__GetTime()}ms")
        return result

    def GetSelfInfo(self) -> THUAI8.Character:
        self.__logger.info(f"GetSelfInfo: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetSelfInfo)

    def __logGetSelfInfo(self) -> THUAI8.Character:
        result = self.__logic.GetSelfInfo()
        if not result:
            self.__logger.warning(f"GetSelfInfo failed at {self.__GetTime()}ms")
        return result

    def Print(self, string: str) -> None:
        self.__logger.info(string)

    def PrintCharacter(self) -> None:
        for char in self.__logic.GetCharacters():
            self.__logger.info("******Character Info******")
            self.__logger.info(
                f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}"
            )
            self.__logger.info(
                f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}"
            )
            self.__logger.info(
                f"CharacterActiveState={char.characterActiveState}, CharacterPassiveState={char.characterPassiveState}"
            )
            self.__logger.info(f"IsBlind={char.isBlind}, BlindTime={char.blindTime}")
            self.__logger.info(f"IsStunned={char.isStunned}, StunTime={char.stunTime}")
            self.__logger.info(
                f"IsInvisible={char.isInvisible}, InvisibleTime={char.invisibleTime}"
            )
            self.__logger.info(f"IsBurned={char.isBurned}, BurnTime={char.burnTime}")
            self.__logger.info(
                f"HarmCut={char.harmCut}, HarmCutTime={char.harmCutTime}"
            )
            self.__logger.info(
                f"CommonAttack={char.commonAttack}, CommonAttackCD={char.commonAttackCD}"
            )
            self.__logger.info(f"SkillAttackCD={char.skillAttackCD}")
            self.__logger.info(f"HP={char.HP}")
            self.__logger.info(f"ShieldEquipment={char.shieldEquipment}")
            self.__logger.info(
                f"ShoesEquipment={char.shoesEquipment}, ShoesTime={char.shoesTime}"
            )
            self.__logger.info(
                f"IsPurified={char.isPurified}, PurifiedTime={char.purifiedTime}"
            )
            self.__logger.info(
                f"IsBerserk={char.isBerserk}, BerserkTime={char.berserkTime}"
            )
            self.__logger.info(
                f"AttackBuffNum={char.attackBuffNum}, AttackBuffTime={char.attackBuffTime}"
            )
            self.__logger.info(f"SpeedBuffTime={char.speedBuffTime}")
            self.__logger.info(f"VisionBuffTime={char.visionBuffTime}")
            self.__logger.info("**************************\n")

    def PrintTeam(self) -> None:
        self.PrintSelfInfo()

    def PrintSelfInfo(self) -> None:
        character = self.__logic.GetSelfInfo()
        self.__logger.info("******Self Info******")
        self.__logger.info(
            f"type={character.characterType}, playerID={character.playerID}, GUID={character.guid}"
        )
        self.__logger.info(
            f"ActiveState={character.characterActiveState}, x={character.x}, y={character.y}"
        )
        self.__logger.info(
            f"speed={character.speed}, view range={character.viewRange}, facing direction={character.facingDirection}"
        )
        self.__logger.info("**************************\n")

    def HaveView(self, gridX: int, gridY: int) -> bool:
        return self.__logic.HaveView(
            gridX,
            gridY,
            self.GetSelfInfo().x,
            self.GetSelfInfo().y,
            self.GetSelfInfo().viewRange,
        )

    def __GetTime(self) -> float:
        return (datetime.datetime.now() - self.__startPoint) / datetime.timedelta(
            milliseconds=1
        )

    def StartTimer(self) -> None:
        self.__startPoint = datetime.datetime.now()
        self.__logger.info("=== AI.play() ===")
        self.__logger.info(f"StartTimer: {self.__startPoint.time()}")

    def EndTimer(self) -> None:
        self.__logger.info(f"Time elapsed: {self.__GetTime()}ms")

    def Play(self, ai: IAI) -> None:
        ai.CharacterPlay(self)


# class CharacterDebugAPI(ICharacterAPI, IGameTimer):
#     def __init__(
#         self,
#         logic: ILogic,
#         file: bool,
#         screen: bool,
#         warnOnly: bool,
#         playerID: int,
#         teamID: int,
#     ) -> None:
#         self.__logic = logic
#         self.__pool = ThreadPoolExecutor(20)
#         self.__startPoint = datetime.datetime.now()
#         self.__logger = logging.getLogger("api " + str(playerID))
#         self.__logger.setLevel(logging.DEBUG)
#         formatter = logging.Formatter(
#             "[%(name)s] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
#             "%H:%M:%S",
#         )

#         # 确保文件存在
#         if not os.path.exists(
#             os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
#         ):
#             os.makedirs(
#                 os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
#             )

#         fileHandler = logging.FileHandler(
#             os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
#             + f"/logs/api-{teamID}-{playerID}-log.txt",
#             mode="w+",
#             encoding="utf-8",
#         )
#         screenHandler = logging.StreamHandler()
#         if file:
#             fileHandler.setLevel(logging.DEBUG)
#             fileHandler.setFormatter(formatter)
#             self.__logger.addHandler(fileHandler)
#         if screen:
#             if warnOnly:
#                 screenHandler.setLevel(logging.WARNING)
#             else:
#                 screenHandler.setLevel(logging.INFO)
#             screenHandler.setFormatter(formatter)
#             self.__logger.addHandler(screenHandler)

#     def StartTimer(self) -> None:
#         self.__startPoint = datetime.datetime.now()
#         self.__logger.info("=== AI.play() ===")
#         self.__logger.info(f"StartTimer: {self.__startPoint.time()}")

#     def EndTimer(self) -> None:
#         self.__logger.info(f"Time elapsed: {self.__GetTime()}ms")

#     def GetFrameCount(self) -> int:
#         return self.__logic.GetCounter()

#     def SendTextMessage(self, toID: int, message: str) -> Future[bool]:
#         self.logger.info(
#             f"SendTextMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSend, toID, message, False)

#     def SendBinaryMessage(self, toID: int, message: bytes) -> Future[bool]:
#         self.logger.info(
#             f"SendBinaryMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSend, toID, message, True)

#     def __logSend(self, toID: int, message: Union[str, bytes], isBinary: bool) -> bool:
#         result = self.__logic.Send(toID, message, isBinary)
#         if not result:
#             self.logger.warning(f"Send failed at {self.__GetTime()}ms")
#         return result

#     def HaveMessage(self) -> bool:
#         self.logger.info(f"HaveMessage: called at {self.__GetTime()}ms")
#         result = self.__logic.HaveMessage()
#         if not result:
#             self.logger.warning(f"HaveMessage failed at {self.__GetTime()}ms")
#         return result

#     def HaveView(self, gridX: int, gridY: int) -> bool:
#         return self.__logic.HaveView(
#             gridX,
#             gridY,
#             self.GetSelfInfo().x,
#             self.GetSelfInfo().y,
#             self.GetSelfInfo().viewRange,
#         )

#     def GetMessage(self) -> Tuple[int, str]:
#         self.logger.info(f"GetMessage: called at {self.__GetTime()}ms")
#         result = self.__logic.GetMessage()
#         if result[0] == -1:
#             self.logger.warning(f"GetMessage failed at {self.__GetTime()}ms")
#         return result

#     def Wait(self) -> bool:
#         self.logger.info(f"Wait: called at {self.__GetTime()}ms")
#         return False if self.__logic.GetCounter() == -1 else self.logic.WaitThread()

#     def Move(self, timeInMilliseconds: int, angleInRadian: float) -> Future[bool]:
#         self.logger.info(
#             f"Move: time={timeInMilliseconds}ms, angle={angleInRadian}rad, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logMove, timeInMilliseconds, angleInRadian)

#     def __logMove(self, time: int, angle: float) -> bool:
#         result = self.__logic.Move(time, angle)
#         if not result:
#             self.__logger.warning(f"Move failed at {self.__GetTime()}ms")
#         return result

#     def MoveDown(self, time: int) -> Future[bool]:
#         return self.Move(time, 0)

#     def MoveRight(self, time: int) -> Future[bool]:
#         return self.Move(time, PI * 0.5)

#     def MoveUp(self, time: int) -> Future[bool]:
#         return self.Move(time, PI)

#     def MoveLeft(self, time: int) -> Future[bool]:
#         return self.Move(time, PI * 1.5)

#     def Skill_Attack(self, angle: float) -> Future[bool]:
#         self.logger.info(
#             f"Skill_Attack: angle={angle}rad, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSkillAttack, angle)

#     def __logSkillAttack(self, angle: float) -> bool:
#         result = self.__logic.SkillAttack(angle)
#         if not result:
#             self.logger.warning(f"Skill_Attack failed at {self.__GetTime()}ms")
#         return result

#     def Common_Attack(self, attackedPlayerID: int, attackedTeamID: int) -> Future[bool]:
#         self.logger.info(
#             f"Common_Attack: attackedPlayerID={attackedPlayerID}, attackedTeamID={attackedTeamID}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(
#             self.__logCommonAttack, attackedPlayerID, attackedTeamID
#         )

#     def __logCommonAttack(self, attackedPlayerID: int, attackedTeamID: int) -> bool:
#         result = self.__logic.CommonAttack(attackedPlayerID, attackedTeamID)
#         if not result:
#             self.logger.warning(f"Common_Attack failed at {self.__GetTime()}ms")
#         return result

#     def Recover(self, recover: int) -> Future[bool]:
#         self.logger.info(f"Recover: {recover}, called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logRecover)

#     def __logRecover(self, recover: int) -> bool:
#         result = self.__logic.Recover(recover)
#         if not result:
#             self.logger.warning(f"Recover failed at {self.__GetTime()}ms")
#         return result

#     def Harvest(self) -> Future[bool]:
#         self.logger.info(f"Harvest: called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logHarvest)

#     def __logHarvest(self) -> bool:
#         result = self.__logic.Harvest()
#         if not result:
#             self.logger.warning(f"Harvest failed at {self.__GetTime()}ms")
#         return result

#     def Rebuild(self, constructionType: ConstructionType) -> Future[bool]:
#         self.logger.info(f"Rebuild: {constructionType}, called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logRebuild, constructionType)

#     def __logRebuild(self, ct: ConstructionType) -> bool:
#         result = self.__logic.Rebuild(ct)
#         if not result:
#             self.logger.warning(f"Rebuild failed at {self.__GetTime()}ms")
#         return result

#     def Construct(self, constructionType: ConstructionType) -> Future[bool]:
#         self.logger.info(
#             f"Construct: {constructionType}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logConstruct, constructionType)

#     def __logConstruct(self, ct: ConstructionType) -> bool:
#         result = self.__logic.Construct(ct)
#         if not result:
#             self.logger.warning(f"Construct failed at {self.__GetTime()}ms")
#         return result

#     def GetCharacters(self) -> List[Character]:
#         self.logger.info(f"GetCharacters: called at {self.__GetTime()}ms")
#         result = self.__logic.GetCharacters()
#         if not result:
#             self.logger.warning(f"GetCharacters failed at {self.__GetTime()}ms")
#         return result

#     def GetEnemyCharacters(self) -> List[Character]:
#         self.logger.info(f"GetEnemyCharacters: called at {self.__GetTime()}ms")
#         result = self.__logic.GetEnemyCharacters()
#         if not result:
#             self.logger.warning(f"GetEnemyCharacters failed at {self.__GetTime()}ms")
#         return result

#     def GetFullMap(self) -> List[List[PlaceType]]:
#         self.logger.info(f"GetFullMap: called at {self.__GetTime()}ms")
#         result = self.__logic.GetFullMap()
#         if not result:
#             self.logger.warning(f"GetFullMap failed at {self.__GetTime()}ms")
#         return result

#     def GetGameInfo(self) -> GameInfo:
#         self.logger.info(f"GetGameInfo: called at {self.__GetTime()}ms")
#         result = self.__logic.GetGameInfo()
#         if not result:
#             self.logger.warning(f"GetGameInfo failed at {self.__GetTime()}ms")
#         return result

#     def GetPlaceType(self, cellX: int, cellY: int) -> Optional[PlaceType]:
#         self.logger.info(
#             f"GetPlaceType: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetPlaceType(cellX, cellY)
#         if not result:
#             self.logger.warning(f"GetPlaceType failed at {self.__GetTime()}ms")
#         return result

#     def GetEnconomyResourceState(
#         self, cellX: int, cellY: int
#     ) -> Optional[EconomyResourceState]:
#         self.logger.info(
#             f"GetEnconomyResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetEnconomyResourceState(cellX, cellY)
#         if not result:
#             self.logger.warning(
#                 f"GetEnconomyResourceState failed at {self.__GetTime()}ms"
#             )
#         return result

#     def GetAdditionResourceState(
#         self, cellX: int, cellY: int
#     ) -> Optional[AdditionResourceState]:
#         self.logger.info(
#             f"GetAdditionResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetAdditionResourceState(cellX, cellY)
#         if not result:
#             self.logger.warning(
#                 f"GetAdditionResourceState failed at {self.__GetTime()}ms"
#             )
#         return result

#     def GetConstructionState(
#         self, cellX: int, cellY: int
#     ) -> Optional[ConstructionState]:
#         self.logger.info(
#             f"GetConstructionState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetConstructionState(cellX, cellY)
#         if not result:
#             self.logger.warning(f"GetConstructionState failed at {self.__GetTime()}ms")
#         return result

#     def GetPlayerGUIDs(self) -> List[int]:
#         self.logger.info(f"GetPlayerGUIDs: called at {self.__GetTime()}ms")
#         result = self.__logic.GetPlayerGUIDs()
#         if not result:
#             self.logger.warning(f"GetPlayerGUIDs failed at {self.__GetTime()}ms")
#         return result

#     def GetEnergy(self) -> int:
#         self.logger.info(f"GetEnergy: called at {self.__GetTime()}ms")
#         result = self.__logic.GetEnergy()
#         if result == -1:
#             self.logger.warning(f"GetEnergy failed at {self.__GetTime()}ms")
#         return result

#     def GetScore(self) -> int:
#         self.logger.info(f"GetScore: called at {self.__GetTime()}ms")
#         result = self.__logic.GetScore()
#         if result == -1:
#             self.logger.warning(f"GetScore failed at {self.__GetTime()}ms")
#         return result

#     def GetSelfInfo(self) -> THUAI8.Character:
#         self.logger.info(f"GetSelfInfo: called at {self.__GetTime()}ms")
#         result = self.__logic.GetSelfInfo()
#         if not result:
#             self.logger.warning(f"GetSelfInfo failed at {self.__GetTime()}ms")
#         return result

#     def Print(self, string: str) -> None:
#         self.logger.info(string)

#     def PrintCharacter(self) -> None:
#         for char in self.__logic.GetCharacters():
#             self.logger.info("******Character Info******")
#             self.logger.info(
#                 f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}"
#             )
#             self.logger.info(
#                 f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}"
#             )
#             self.logger.info("**************************")

#     def PrintSelfInfo(self) -> None:
#         selfInfo = self.GetSelfInfo()
#         self.logger.info("******Self Info******")
#         self.logger.info(
#             f"type={selfInfo.characterType}, ID={selfInfo.characterID}, GUID={selfInfo.guid}"
#         )
#         self.logger.info(
#             f"x={selfInfo.x}, y={selfInfo.y}, state={selfInfo.characterState}"
#         )
#         self.logger.info("*********************")

#     def EndAllAction(self) -> Future[bool]:
#         self.logger.info(f"EndAllAction: called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.logic.EndAllAction)

#     def __GetTime(self) -> float:
#         return (datetime.datetime.now() - self.startPoint).total_seconds() * 1000

#     def Play(self, ai: IAI) -> None:
#         ai.play(self)


class TeamDebugAPI(ITeamAPI, IGameTimer):
    def __init__(
        self,
        logic: ILogic,
        file: bool,
        screen: bool,
        warnOnly: bool,
        playerID: int,
        teamID: int,
    ) -> None:
        self.__logic = logic
        self.__pool = ThreadPoolExecutor(20)
        self.__startPoint = datetime.datetime.now()
        self.__logger = logging.getLogger("api " + str(playerID))
        self.__logger.setLevel(logging.DEBUG)
        formatter = logging.Formatter(
            "[%(name)s] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
            "%H:%M:%S",
        )
        # 确保文件存在
        if not os.path.exists(
            os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
        ):
            os.makedirs(
                os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
            )

        fileHandler = logging.FileHandler(
            os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
            + f"/logs/api-{teamID}-{playerID}-log.txt",
            mode="w+",
            encoding="utf-8",
        )
        screenHandler = logging.StreamHandler()
        if file:
            fileHandler.setLevel(logging.DEBUG)
            fileHandler.setFormatter(formatter)
            self.__logger.addHandler(fileHandler)
        if screen:
            if warnOnly:
                screenHandler.setLevel(logging.WARNING)
            else:
                screenHandler.setLevel(logging.INFO)
            screenHandler.setFormatter(formatter)
            self.__logger.addHandler(screenHandler)

    def __GetTime(self) -> float:
        return (datetime.datetime.now() - self.__startPoint) / datetime.timedelta(
            milliseconds=1
        )

    def StartTimer(self) -> None:
        self.__startPoint = datetime.datetime.now()
        self.__logger.info("=== AI.play() ===")
        self.__logger.info(f"StartTimer: {self.__startPoint.time()}")

    def EndTimer(self) -> None:
        self.__logger.info(f"Time elapsed: {self.__GetTime()}ms")

    def SendMessage(self, toID: int, message: Union[str, bytes]) -> Future[bool]:
        self.__logger.info(
            f"SendMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logSend, toID, message, False)

    def __logSend(self, toID: int, message: Union[str, bytes], isBinary: bool) -> bool:
        result = self.__logic.Send(toID, message, isBinary)
        if not result:
            self.__logger.warning(f"Send failed at {self.__GetTime()}ms")
        return result

    def HaveMessage(self) -> bool:
        self.logger.info(f"HaveMessage: called at {self.__GetTime()}ms")
        result = self.__logic.HaveMessage()
        if not result:
            self.__logger.warning(f"HaveMessage failed at {self.__GetTime()}ms")
        return result

    def GetMessage(self) -> Tuple[int, Union[str, bytes]]:
        self.__logger.info(f"GetMessage: called at {self.__GetTime()}ms")
        result = self.__logic.GetMessage()
        if result[0] == -1:
            self.__logger.warning(f"GetMessage failed at {self.__GetTime()}ms")
        return result

    def Wait(self) -> bool:
        self.__logger.info(f"Wait: called at {self.__GetTime()}ms")
        if self.__logic.GetCounter() == -1:
            return False
        else:
            return self.__logic.WaitThread()

    def EndAllAction(self) -> Future[bool]:
        self.__logger.info(f"EndAllAction: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logEnd)

    def __logEnd(self) -> bool:
        result = self.__logic.EndAllAction()
        if not result:
            self.__logger.warning(f"EndAllAction failed at {self.__GetTime()}ms")
        return result

    def GetCharacters(self) -> List[THUAI8.Character]:
        self.__logger.info(f"GetCharacters: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetCharacters)

    def __logGetCharacters(self) -> List[THUAI8.Character]:
        result = self.__logic.GetCharacters()
        if not result:
            self.__logger.warning(f"GetCharacters failed at {self.__GetTime()}ms")
        return result

    def GetEnemyCharacters(self) -> List[THUAI8.Character]:
        self.__logger.info(f"GetEnemyCharacters: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetEnemyCharacters)

    def __logGetEnemyCharacters(self) -> List[THUAI8.Character]:
        result = self.__logic.GetEnemyCharacters()
        if not result:
            self.__logger.warning(f"GetEnemyCharacters failed at {self.__GetTime()}ms")
        return result

    def GetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        self.__logger.info(f"GetFullMap: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetFullMap)

    def __logGetFullMap(self) -> List[List[THUAI8.PlaceType]]:
        result = self.__logic.GetFullMap()
        if not result:
            self.__logger.warning(f"GetFullMap failed at {self.__GetTime()}ms")
        return result

    def GetGameInfo(self) -> THUAI8.GameInfo:
        self.__logger.info(f"GetGameInfo: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetGameInfo)

    def __logGetGameInfo(self) -> THUAI8.GameInfo:
        result = self.__logic.GetGameInfo()
        if not result:
            self.__logger.warning(f"GetGameInfo failed at {self.__GetTime()}ms")
        return result

    def GetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        self.__logger.info(
            f"GetPlaceType: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__lpool.submit(self.__logGetPlaceType, cellX, cellY)

    def __logGetPlaceType(self, cellX: int, cellY: int) -> THUAI8.PlaceType:
        result = self.__logic.GetPlaceType(cellX, cellY)
        if not result:
            self.__logger.warning(f"GetPlaceType failed at {self.__GetTime()}ms")
        return result

    def GetEconomyResourceState(self, cellX: int, cellY: int) -> int:
        self.__logger.info(
            f"GetEconomyResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetEconomyResourceState, cellX, cellY)

    def __logGetEconomyResourceState(self, cellX: int, cellY: int) -> int:
        result = self.__logic.GetEconomyResourceState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetEconomyResourceState failed at {self.__GetTime()}ms"
            )
        return result

    def GetAdditionResourceState(self, cellX: int, cellY: int) -> int:
        self.__logger.info(
            f"GetAdditionResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetAdditionResourceState, cellX, cellY)

    def __logGetAdditionResourceState(self, cellX: int, cellY: int) -> int:
        result = self.__logic.GetAdditionResourceState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetAdditionResourceState failed at {self.__GetTime()}ms"
            )
        return result

    def GetConstructionState(self, cellX: int, cellY: int) -> int:
        self.__logger.info(
            f"GetConstructionState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logGetConstructionState, cellX, cellY)

    def __logGetConstructionState(self, cellX: int, cellY: int) -> int:
        result = self.__logic.GetConstructionState(cellX, cellY)
        if not result:
            self.__logger.warning(
                f"GetConstructionState failed at {self.__GetTime()}ms"
            )
        return result

    def Recycle(self, cellX: int, cellY: int) -> Future[bool]:
        self.__logger.info(
            f"Recycle: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logRecycle, cellX, cellY)

    def __logRecycle(self, cellX: int, cellY: int) -> bool:
        result = self.__logic.Recycle(cellX, cellY)
        if not result:
            self.__logger.warning(f"Recycle failed at {self.__GetTime()}ms")
        return result

    def GetFrameCount(self) -> int:
        self.__logger.info(f"GetFrameCount: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetFrameCount)

    def __logGetFrameCount(self) -> int:
        result = self.__logic.GetCounter()
        if result == -1:
            self.__logger.warning(f"GetFrameCount failed at {self.__GetTime()}ms")
        return result

    def GetPlayerGUIDs(self) -> List[int]:
        self.__logger.info(f"GetPlayerGUIDs: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetPlayerGUIDs)

    def __logGetPlayerGUIDs(self) -> List[int]:
        result = self.__logic.GetPlayerGUIDs()
        if not result:
            self.__logger.warning(f"GetPlayerGUIDs failed at {self.__GetTime()}ms")
        return result

    def GetEnergy(self) -> int:
        self.__logger.info(f"GetEnergy: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetEnergy)

    def __logGetEnergy(self) -> int:
        result = self.__logic.GetEnergy()
        if result == -1:
            self.__logger.warning(f"GetEnergy failed at {self.__GetTime()}ms")
        return result

    def GetScore(self) -> int:
        self.__logger.info(f"GetScore: called at {self.__GetTime()}ms")
        return self.__pool.submit(self.__logGetScore)

    def __logGetScore(self) -> int:
        result = self.__logic.GetScore()
        if result == -1:
            self.__logger.warning(f"GetScore failed at {self.__GetTime()}ms")
        return result

    def GetSelfInfo(self) -> THUAI8.Home:
        self.__logger.info(f"GetSelfInfo: called at {self.__GetTime()}ms")
        return cast(THUAI8.Home, self.__logic.GetSelfInfo())

    # def __logGetSelfInfo(self) -> THUAI8.TeamInfo:
    #     result = self.__logic.GetSelfInfo()
    #     if not result:
    #         self.__logger.warning(f"GetSelfInfo failed at {self.__GetTime()}ms")
    #     return result

    def InstallEquipment(
        self, playerID: int, equipmentType: THUAI8.EquipmentType
    ) -> Future[bool]:
        self.__logger.info(
            f"InstallEquipment: playerID={playerID}, type={equipmentType}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(self.__logInstall, playerID, equipmentType)

    def __logInstall(self, playerID: int, equipmentType: THUAI8.EquipmentType) -> bool:
        result = self.__logic.InstallEquipment(playerID, equipmentType)
        if not result:
            self.__logger.warning(f"InstallEquipment failed at {self.__GetTime()}ms")
        return result

    def BuildCharacter(
        self, playerID: int, characterType: THUAI8.CharacterType, x: int, y: int
    ) -> Future[bool]:
        self.__logger.info(
            f"BuildCharacter: playerID={playerID}, type={characterType}, x={x}, y={y}, called at {self.__GetTime()}ms"
        )
        return self.__pool.submit(
            self.__logBuildCharacter, playerID, characterType, x, y
        )

    def __logBuildCharacter(
        self, playerID: int, characterType: THUAI8.CharacterType, x: int, y: int
    ) -> bool:
        result = self.__logic.BuildCharacter(playerID, characterType, x, y)
        if not result:
            self.__logger.warning(f"BuildCharacter failed at {self.__GetTime()}ms")
        return result

    def PrintSelfInfo(self) -> None:
        selfInfo = self.GetSelfInfo()
        self.__logger.info("******Self Info******")
        self.__logger.info(
            f"teamID:{selfInfo.teamID} playerID:{selfInfo.playerID} score:{selfInfo.score} energy:{selfInfo.energy}"
        )
        self.__logger.info("*********************")

    def Print(self, string: str) -> None:
        self.__logger.info(string)

    def PrintCharacter(self) -> None:
        for char in self.__logic.GetCharacters():
            self.__logger.info("******Character Info******")
            self.__logger.info(
                f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}"
            )
            self.__logger.info(
                f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}"
            )
            self.__logger.info("**************************")

    def PrintTeam(self) -> None:
        for char in self.__logic.GetCharacters():
            self.__logger.info("******Team Info******")
            self.__logger.info(
                f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}"
            )
            self.__logger.info(
                f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}"
            )
            self.__logger.info("**************************")

    def Play(self, ai: IAI) -> None:
        ai.TeamPlay(self)


# class TeamDebugAPI(CharacterDebugAPI):
#     def __init__(
#         self,
#         logic: ILogic,
#         file: bool,
#         screen: bool,
#         warnOnly: bool,
#         playerID: int,
#         teamID: int,
#     ) -> None:
#         self.__logic = logic
#         self.__pool = ThreadPoolExecutor(20)
#         self.__startPoint = datetime.datetime.now()
#         self.__logger = logging.getLogger("api " + str(playerID))
#         self.__logger.setLevel(logging.DEBUG)
#         formatter = logging.Formatter(
#             "[%(name)s] [%(asctime)s.%(msecs)03d] [%(levelname)s] %(message)s",
#             "%H:%M:%S",
#         )
#         # 确保文件存在
#         if not os.path.exists(
#             os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
#         ):
#             os.makedirs(
#                 os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/logs"
#             )

#         fileHandler = logging.FileHandler(
#             os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
#             + f"/logs/api-{teamID}-{playerID}-log.txt",
#             mode="w+",
#             encoding="utf-8",
#         )
#         screenHandler = logging.StreamHandler()
#         if file:
#             fileHandler.setLevel(logging.DEBUG)
#             fileHandler.setFormatter(formatter)
#             self.__logger.addHandler(fileHandler)
#         if screen:
#             if warnOnly:
#                 screenHandler.setLevel(logging.WARNING)
#             else:
#                 screenHandler.setLevel(logging.INFO)
#             screenHandler.setFormatter(formatter)
#             self.__logger.addHandler(screenHandler)

#     def StartTimer(self) -> None:
#         self.__startPoint = datetime.datetime.now()
#         self.__logger.info("=== AI.play() ===")
#         self.__logger.info(f"StartTimer: {self.__startPoint.time()}")

#     def EndTimer(self) -> None:
#         self.__logger.info(f"Time elapsed: {self.__GetTime()}ms")

#     def GetFrameCount(self) -> int:
#         return self.__logic.GetCounter()

#     def SendTextMessage(self, toID: int, message: str) -> Future[bool]:
#         self.logger.info(
#             f"SendTextMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSend, toID, message, False)

#     def SendBinaryMessage(self, toID: int, message: bytes) -> Future[bool]:
#         self.logger.info(
#             f"SendBinaryMessage: toID={toID}, message={message}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSend, toID, message, True)

#     def __logSend(self, toID: int, message: Union[str, bytes], isBinary: bool) -> bool:
#         result = self.__logic.Send(toID, message, isBinary)
#         if not result:
#             self.logger.warning(f"Send failed at {self.__GetTime()}ms")
#         return result

#     def HaveMessage(self) -> bool:
#         self.logger.info(f"HaveMessage: called at {self.__GetTime()}ms")
#         result = self.__logic.HaveMessage()
#         if not result:
#             self.logger.warning(f"HaveMessage failed at {self.__GetTime()}ms")
#         return result

#     def HaveView(self, gridX: int, gridY: int) -> bool:
#         return self.__logic.HaveView(
#             gridX,
#             gridY,
#             self.GetSelfInfo().x,
#             self.GetSelfInfo().y,
#             self.GetSelfInfo().viewRange,
#         )

#     def GetMessage(self) -> Tuple[int, str]:
#         self.logger.info(f"GetMessage: called at {self.__GetTime()}ms")
#         result = self.__logic.GetMessage()
#         if result[0] == -1:
#             self.logger.warning(f"GetMessage failed at {self.__GetTime()}ms")
#         return result

#     def Wait(self) -> bool:
#         self.logger.info(f"Wait: called at {self.__GetTime()}ms")
#         return False if self.__logic.GetCounter() == -1 else self.logic.WaitThread()

#     def Move(self, timeInMilliseconds: int, angleInRadian: float) -> Future[bool]:
#         self.logger.info(
#             f"Move: time={timeInMilliseconds}ms, angle={angleInRadian}rad, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logMove, timeInMilliseconds, angleInRadian)

#     def __logMove(self, time: int, angle: float) -> bool:
#         result = self.__logic.Move(time, angle)
#         if not result:
#             self.__logger.warning(f"Move failed at {self.__GetTime()}ms")
#         return result

#     def MoveDown(self, time: int) -> Future[bool]:
#         return self.Move(time, 0)

#     def MoveRight(self, time: int) -> Future[bool]:
#         return self.Move(time, PI * 0.5)

#     def MoveUp(self, time: int) -> Future[bool]:
#         return self.Move(time, PI)

#     def MoveLeft(self, time: int) -> Future[bool]:
#         return self.Move(time, PI * 1.5)

#     def Skill_Attack(self, angle: float) -> Future[bool]:
#         self.logger.info(
#             f"Skill_Attack: angle={angle}rad, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logSkillAttack, angle)

#     def __logSkillAttack(self, angle: float) -> bool:
#         result = self.__logic.SkillAttack(angle)
#         if not result:
#             self.logger.warning(f"Skill_Attack failed at {self.__GetTime()}ms")
#         return result

#     def Common_Attack(self, attackedPlayerID: int, attackedTeamID: int) -> Future[bool]:
#         self.logger.info(
#             f"Common_Attack: attackedPlayerID={attackedPlayerID}, attackedTeamID={attackedTeamID}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(
#             self.__logCommonAttack, attackedPlayerID, attackedTeamID
#         )

#     def __logCommonAttack(self, attackedPlayerID: int, attackedTeamID: int) -> bool:
#         result = self.__logic.CommonAttack(attackedPlayerID, attackedTeamID)
#         if not result:
#             self.logger.warning(f"Common_Attack failed at {self.__GetTime()}ms")
#         return result

#     def Recover(self, recover: int) -> Future[bool]:
#         self.logger.info(f"Recover: {recover}, called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logRecover)

#     def __logRecover(self, recover: int) -> bool:
#         result = self.__logic.Recover(recover)
#         if not result:
#             self.logger.warning(f"Recover failed at {self.__GetTime()}ms")
#         return result

#     def Harvest(self) -> Future[bool]:
#         self.logger.info(f"Harvest: called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logHarvest)

#     def __logHarvest(self) -> bool:
#         result = self.__logic.Harvest()
#         if not result:
#             self.logger.warning(f"Harvest failed at {self.__GetTime()}ms")
#         return result

#     def Rebuild(self, constructionType: ConstructionType) -> Future[bool]:
#         self.logger.info(f"Rebuild: {constructionType}, called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.__logRebuild, constructionType)

#     def __logRebuild(self, ct: ConstructionType) -> bool:
#         result = self.__logic.Rebuild(ct)
#         if not result:
#             self.logger.warning(f"Rebuild failed at {self.__GetTime()}ms")
#         return result

#     def Construct(self, constructionType: ConstructionType) -> Future[bool]:
#         self.logger.info(
#             f"Construct: {constructionType}, called at {self.__GetTime()}ms"
#         )
#         return self.__pool.submit(self.__logConstruct, constructionType)

#     def __logConstruct(self, ct: ConstructionType) -> bool:
#         result = self.__logic.Construct(ct)
#         if not result:
#             self.logger.warning(f"Construct failed at {self.__GetTime()}ms")
#         return result

#     def GetCharacters(self) -> List[Character]:
#         self.logger.info(f"GetCharacters: called at {self.__GetTime()}ms")
#         result = self.__logic.GetCharacters()
#         if not result:
#             self.logger.warning(f"GetCharacters failed at {self.__GetTime()}ms")
#         return result

#     def GetEnemyCharacters(self) -> List[Character]:
#         self.logger.info(f"GetEnemyCharacters: called at {self.__GetTime()}ms")
#         result = self.__logic.GetEnemyCharacters()
#         if not result:
#             self.logger.warning(f"GetEnemyCharacters failed at {self.__GetTime()}ms")
#         return result

#     def GetFullMap(self) -> List[List[PlaceType]]:
#         self.logger.info(f"GetFullMap: called at {self.__GetTime()}ms")
#         result = self.__logic.GetFullMap()
#         if not result:
#             self.logger.warning(f"GetFullMap failed at {self.__GetTime()}ms")
#         return result

#     def GetGameInfo(self) -> GameInfo:
#         self.logger.info(f"GetGameInfo: called at {self.__GetTime()}ms")
#         result = self.__logic.GetGameInfo()
#         if not result:
#             self.logger.warning(f"GetGameInfo failed at {self.__GetTime()}ms")
#         return result

#     def GetPlaceType(self, cellX: int, cellY: int) -> Optional[PlaceType]:
#         self.logger.info(
#             f"GetPlaceType: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetPlaceType(cellX, cellY)
#         if not result:
#             self.logger.warning(f"GetPlaceType failed at {self.__GetTime()}ms")
#         return result

#     def GetEnconomyResourceState(
#         self, cellX: int, cellY: int
#     ) -> Optional[EconomyResourceState]:
#         self.logger.info(
#             f"GetEnconomyResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetEnconomyResourceState(cellX, cellY)
#         if not result:
#             self.logger.warning(
#                 f"GetEnconomyResourceState failed at {self.__GetTime()}ms"
#             )
#         return result

#     def GetAdditionResourceState(
#         self, cellX: int, cellY: int
#     ) -> Optional[AdditionResourceState]:
#         self.logger.info(
#             f"GetAdditionResourceState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetAdditionResourceState(cellX, cellY)
#         if not result:
#             self.logger.warning(
#                 f"GetAdditionResourceState failed at {self.__GetTime()}ms"
#             )
#         return result

#     def GetConstructionState(
#         self, cellX: int, cellY: int
#     ) -> Optional[ConstructionState]:
#         self.logger.info(
#             f"GetConstructionState: cellX={cellX}, cellY={cellY}, called at {self.__GetTime()}ms"
#         )
#         result = self.__logic.GetConstructionState(cellX, cellY)
#         if not result:
#             self.logger.warning(f"GetConstructionState failed at {self.__GetTime()}ms")
#         return result

#     def GetPlayerGUIDs(self) -> List[int]:
#         self.logger.info(f"GetPlayerGUIDs: called at {self.__GetTime()}ms")
#         result = self.__logic.GetPlayerGUIDs()
#         if not result:
#             self.logger.warning(f"GetPlayerGUIDs failed at {self.__GetTime()}ms")
#         return result

#     def GetEnergy(self) -> int:
#         self.logger.info(f"GetEnergy: called at {self.__GetTime()}ms")
#         result = self.__logic.GetEnergy()
#         if result == -1:
#             self.logger.warning(f"GetEnergy failed at {self.__GetTime()}ms")
#         return result

#     def GetScore(self) -> int:
#         self.logger.info(f"GetScore: called at {self.__GetTime()}ms")
#         result = self.__logic.GetScore()
#         if result == -1:
#             self.logger.warning(f"GetScore failed at {self.__GetTime()}ms")
#         return result

#     def GetSelfInfo(self) -> Character:
#         self.logger.info(f"GetSelfInfo: called at {self.__GetTime()}ms")
#         result = self.__logic.GetSelfInfo()
#         if not result:
#             self.logger.warning(f"GetSelfInfo failed at {self.__GetTime()}ms")
#         return result

#     def Print(self, string: str) -> None:
#         self.logger.info(string)

#     def PrintCharacter(self) -> None:
#         for char in self.__logic.GetCharacters():
#             self.logger.info("******Character Info******")
#             self.logger.info(
#                 f"type={char.characterType}, ID={char.characterID}, GUID={char.guid}, x={char.x}, y={char.y}"
#             )
#             self.logger.info(
#                 f"state={char.characterState}, speed={char.speed}, view={char.viewRange}, facing={char.facingDirection}"
#             )
#             self.logger.info("**************************")

#     def PrintSelfInfo(self) -> None:
#         selfInfo = self.GetSelfInfo()
#         self.logger.info("******Self Info******")
#         self.logger.info(
#             f"type={selfInfo.characterType}, ID={selfInfo.characterID}, GUID={selfInfo.guid}"
#         )
#         self.logger.info(
#             f"x={selfInfo.x}, y={selfInfo.y}, state={selfInfo.characterState}"
#         )
#         self.logger.info("*********************")

#     def EndAllAction(self) -> Future[bool]:
#         self.logger.info(f"EndAllAction: called at {self.__GetTime()}ms")
#         return self.__pool.submit(self.logic.EndAllAction)

#     def __GetTime(self) -> float:
#         return (datetime.datetime.now() - self.startPoint).total_seconds() * 1000

#     def Play(self, ai: IAI) -> None:
#         ai.play(self)

#     # def InstallEquipment(self, playerID: int, equipmentType: Any) -> Future[bool]:
#     #     self.logger.info(
#     #         f"InstallEquipment: playerID={playerID}, type={equipmentType}, called at {self.__GetTime()}ms"
#     #     )
#     #     return self.__pool.submit(self.__logInstall, playerID, equipmentType)

#     # def __logInstall(self, pid: int, et: Any) -> bool:
#     #     result = self.__logic.InstallEquipment(pid, et)
#     #     if not result:
#     #         self.logger.warning(f"InstallEquipment failed at {self.__GetTime()}ms")
#     #     return result

#     # def BuildCharacter(
#     #     self, characterType: CharacterType, birthIndex: int
#     # ) -> Future[bool]:
#     #     self.logger.info(
#     #         f"BuildCharacter: type={characterType}, index={birthIndex}, called at {self.__GetTime()}ms"
#     #     )
#     #     return self.__pool.submit(self.__logBuild, characterType, birthIndex)

#     # def __logBuild(self, ct: CharacterType, bi: int) -> bool:
#     #     result = self.__logic.BuildCharacter(ct, bi)
#     #     if not result:
#     #         self.logger.warning(f"BuildCharacter failed at {self.__GetTime()}ms")
#     #     return result

#     # def PrintSelfInfo(self) -> None:
#     #     selfInfo = self.logic.GetSelfInfo()
#     #     self.logger.info("******Team Info******")
#     #     self.logger.info(f"teamID={selfInfo.teamID}, playerID={selfInfo.playerID}")
#     #     self.logger.info(f"score={selfInfo.score}, energy={selfInfo.energy}")
#     #     self.logger.info("*********************")
