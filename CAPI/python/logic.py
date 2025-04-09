import copy
import logging
import os
import platform
import threading
from queue import Queue
from typing import Callable, List, Tuple, Union

import proto.Message2Clients_pb2 as Message2Clients
import proto.Message2Server_pb2 as Message2Server
import proto.MessageType_pb2 as MessageType
import PyAPI.structures as THUAI8
from PyAPI.AI import Setting
from PyAPI.API import CharacterAPI, TeamAPI
from PyAPI.Communication import Communication
from PyAPI.DebugAPI import CharacterDebugAPI, TeamDebugAPI
from PyAPI.Interface import IGameTimer, ILogic
from PyAPI.State import State
from PyAPI.utils import AssistFunction, Proto2THUAI8


class Logic(ILogic):
    def __init__(
        self,
        playerID: int,
        teamID: int,
        playerType: THUAI8.PlayerType,
        characterType: THUAI8.CharacterType,
    ) -> None:
        self.__playerID: int = playerID
        self.__teamID: int = teamID
        self.__playerType: THUAI8.PlayerType = playerType
        self.__characterType: THUAI8.CharacterType = characterType

        self.__comm: Communication

        self.__currentState: State = State()
        self.__bufferState: State = State()

        self.__timer: IGameTimer

        self.__threadAI: threading.Thread

        self.__mtxState: threading.Lock = threading.Lock()

        self.__cvBuffer: threading.Condition = threading.Condition()
        self.__cvAI: threading.Condition = threading.Condition()

        self.__counterState: int = 0
        self.__counterBuffer: int = 0

        self.__gameState: THUAI8.GameState = THUAI8.GameState.NullGameState

        self.__AILoop: bool = True

        self.__bufferUpdated: bool = False

        self.__AIStart: bool = False

        self.__freshed: bool = False

        self.__logger: logging.Logger = logging.getLogger("Logic")

        self.__messageQueue: Queue = Queue()

        def GetChracters(self) -> List[THUAI8.Character]:
            with self.__mtxState:
                self.__logger.debug("Called GetCharacters")
                return copy.deepcopy(self.__currentState.characters)

        def GetEnemyCharacters(self) -> List[THUAI8.Character]:
            with self.__mtxState:
                self.__logger.debug("Called GetEnemyCharacters")
                return copy.deepcopy(self.__currentState.enemyCharacters)

        def GetSelfInfo(self) -> Union[THUAI8.Character, THUAI8.Team]:
            with self.__mtxState:
                self.__logger.debug("Called GetSelfInfo")
                return copy.deepcopy(self.__currentState.self)

        def GetFullMap(self) -> List[List[THUAI8.PlaceType]]:
            with self.__mtxState:
                self.__logger.debug("Called GetFullMap")
                return copy.deepcopy(self.__currentState.gameMap)

        def GetPlaceType(self, x: int, y: int) -> THUAI8.PlaceType:
            with self.__mtxState:
                if (
                    x < 0
                    or x >= len(self.__currentState.gameMap)
                    or y < 0
                    or y >= len(self.__currentState.gameMap[0])
                ):
                    self.__logger.warning("GetPlaceType: Out of range")
                    return THUAI8.PlaceType(0)
                self.__logger.debug("Called GetPlaceType")
                return copy.deepcopy(self.__currentState.gameMap[x][y])

        def GetGameInfo(self) -> THUAI8.GameInfo:
            with self.__mtxState:
                self.__logger.debug("Called GetGameInfo")
                return copy.deepcopy(self.__currentState.gameInfo)

        def Move(self, time: int, angle: float) -> bool:
            self.__logger.debug("Called Move")
            return self.__comm.Move(time, angle, self.__playerID, self.__teamID)

        def SendMessage(self, toID: int, message: Union[str, bytes]) -> bool:
            self.__logger.debug("Called SendMessage")
            return self.__comm.SendMessage(
                toID, message, self.__playerID, self.__teamID
            )

        def HaveMessage(self) -> bool:
            self.__logger.debug("Called HaveMessage")
            return not self.__messageQueue.empty()

        def GetMessage(self) -> Tuple[int, Union[str, bytes]]:
            self.__logger.debug("Called GetMessage")
            if self.__messageQueue.empty():
                self.__logger.warning("GetMessage: No message")
                return -1, ""
            else:
                return self.__messageQueue.get()

        def WaitThread(self) -> bool:
            if Setting.Asynchronous():
                self.__Wait()
            return True

        def GetCounter(self) -> int:
            with self.__mtxState:
                return copy.deepcopy(self.__counterState)

        def GetPlayerGUIDs(self) -> List[int]:
            with self.__mtxState:
                return copy.deepcopy(self.__currentState.guids)

        def GetConstructionState(
            self, cellX: int, cellY: int
        ) -> THUAI8.ConstructionState | None:
            with self.__mtxState:
                self.__logger.debug("Called GetConstructionState")
                if (cellX, cellY) in self.__currentState.mapInfo.barracksState:
                    return THUAI8.ConstructionState(
                        self.__currentState.mapInfo.barracksState[(cellX, cellY)],
                        THUAI8.ConstructionType.Barracks,
                    )
                elif (cellX, cellY) in self.__currentState.mapInfo.springState:
                    return THUAI8.ConstructionState(
                        self.__currentState.mapInfo.springState[(cellX, cellY)],
                        THUAI8.ConstructionType.Spring,
                    )
                elif (cellX, cellY) in self.__currentState.mapInfo.farmState:
                    return THUAI7.ConstructionState(
                        self.__currentState.mapInfo.farmState[(cellX, cellY)],
                        THUAI7.ConstructionType.Farm,
                    )
                else:
                    self.__logger.warning("GetConstructionState: Out of range")
                    return None

        def GetEconomyResourceState(self, cellX: int, cellY: int) -> int:
            with self.__mtxState:
                self.__logger.debug("Called GetEconomyResourceState")
                if (
                    cellX,
                    cellY,
                ) not in self.__currentState.mapInfo.economyResourceState:
                    self.__logger.warning("GetEconomyResourceState: Out of range")
                    return -1
                else:
                    return copy.deepcopy(
                        self.__currentState.mapInfo.economyResourceState[(cellX, cellY)]
                    )

        def GetAdditionResourceState(self, cellX: int, cellY: int) -> int:
            with self.__mtxState:
                self.__logger.debug("Called GetAdditionResourceState")
                if (
                    cellX,
                    cellY,
                ) not in self.__currentState.mapInfo.additionResourceState:
                    self.__logger.warning("GetAdditionResourceState: Out of range")
                    return -1
                else:
                    return copy.deepcopy(
                        self.__currentState.mapInfo.additionResourceState[
                            (cellX, cellY)
                        ]
                    )

        def GetEconomy(self) -> int:
            with self.__mtxState:
                self.__logger.debug("Called GetEconomy")
                return copy.deepcopy(
                    self.__currentState.gameInfo.buddhistsTeamEconomy
                    if self.__teamID == 0
                    else self.__currentState.gameInfo.monstersTeamEconomy
                )

        def GetScore(self) -> int:
            with self.__mtxState:
                self.__logger.debug("Called GetScore")
                return copy.deepcopy(
                    self.__currentState.gameInfo.buddhistsTeamEconomy
                    if self.__teamID == 0
                    else self.__currentState.gameInfo.monstersTeamEconomy
                )

        def CommonAttack(
            self, playerID: int, teamID: int, ATKplayerID: int, ATKteamID: int
        ) -> bool:
            self.__logger.debug("Called CommonAttack")
            return self.__comm.Attack(playerID, teamID, ATKplayerID, ATKteamID)

        def SkillAttack(self, playerID: int, teamID: int, angle: float) -> bool:
            self.__logger.debug("Called CommonAttack")
            return self.__comm.Attack(playerID, teamID, ATKplayerID, ATKteamID)

        def Recover(self, recover: int) -> bool:
            self.__logger.debug("Called Recover")
            return self.__comm.Recover(self.__playerID, self.__teamID, recover)

        def Construct(self, constructionType: THUAI8.ConstructionType) -> bool:
            self.__logger.debug("Called Construct")
            return self.__comm.Construct(
                constructionType, self.__playerID, self.__teamID
            )

        def BuildCharacter(
            self, CharacterType: THUAI8.CharacterType, birthIndex: int
        ) -> bool:
            self.__logger.debug("Called BuildCharacter")
            return self.__comm.BuildCharacter(self.__teamID, characterType, birthIndex)

        def Rebuild(self, constructionType: THUAI8.ConstructionType) -> bool:
            self.__logger.debug("Called Rebuild")
            return self.__comm.Rebuild(constructionType, self.__playerID, self.__teamID)

        def InstallEquipment(
            self, playerID: int, equipmentType: THUAI8.EquipmentType
        ) -> bool:
            self.__logger.debug("Called InstallEquipment")
            return self.__comm.InstallEquipment(equipmentType, playerID, self.__teamID)

        def Recycle(self, playerID: int) -> bool:
            self.__logger.debug("Called Recycle")
            return self.__comm.Recycle(playerID, self.__teamID)

        def EndAllAction(self) -> bool:
            self.__logger.debug("Called EndAllAction")
            return self.__comm.EndAllAction(self.__playerID, self.__teamID)

        def HaveView(
            self, gridX: int, gridY: int, selfX: int, selfY: int, viewRange: int
        ) -> bool:
            with self.__mtxState:
                self.__logger.debug("Called HaveView")
                return AssistFunction.HaveView(
                    viewRange, selfX, selfY, gridX, gridY, self.__currentState.gameMap
                )

        def TryConnection(self) -> bool:
            self.__logger.info("Called TryConnection")
            return self.__comm.TryConnection(self.__playerID, self.__teamID)

        def __TryConnection(self) -> bool:
            self.__logger.info("Try to connect to the server.")
            return self.__comm.TryConnection(self.__playerID, self.__teamID)

        def __ProcessMessage(self) -> None:
            def messageThread():
                self.__logger.info("Message thread started")
                self.__comm.AddPlayer(
                    self.__playerID, self.__teamID, self.__CharacterType
                )
                self.__logger.info("Player added")

                while self.__gameState != THUAI8.GameState.GameEnd:
                    clientMsg = self.__comm.GetMessage2Client()
                    self.__logger.debug("Get message from server")
                    self.__gameState = Proto2THUAI8.gameStateDict[clientMsg.game_state]

                    if self.__gameState == THUAI8.GameState.GameStart:
                        self.__logger.info("Game start!")

                        for obj in clientMsg.obj_message:
                            if obj.WhichOneof("message_of_obj") == "map_message":
                                gameMap: List[List[THUAI8.PlaceType]] = []
                                for row in obj.map_message.rows:
                                    cols: List[THUAI8.PlaceType] = []
                                    for place in row.cols:
                                        cols.append(Proto2THUAI8.placeTypeDict[place])
                                    gameMap.append(cols)
                                self.__currentState.gameMap = gameMap
                                self.__bufferState.gameMap = gameMap
                                self.__logger.info("Game map loaded!")
                                break
                        else:
                            self.__logger.error("No map message received")

                        self.__LoadBuffer(clientMsg)
                        self.__AILoop = True
                        self.__UnBlockAI()

                    elif self.__gameState == THUAI8.GameState.GameRunning:
                        # 读取玩家的GUID
                        self.__LoadBuffer(clientMsg)
                    else:
                        self.__logger.error("Unknown GameState!")
                        continue
                with self.__cvBuffer:
                    self.__bufferUpdated = True
                    self.__counterBuffer = -1
                    self.__cvBuffer.notify()
                    self.__logger.info("Game End!")
                self.__logger.info("Message thread end!")
                self.__AILoop = False

            threading.Thread(target=messageThread).start()

        def __LoadBuffer(self, message: Message2Clients.MessageToClient) -> None:
            with self.__cvBuffer:
                self.__bufferState.characters.clear()
                self.__bufferState.enemyCharacters.clear()
                # self.__bufferState.bullets.clear()
                # self.__bufferState.bombedBullets.clear()
                self.__bufferState.guids.clear()
                self.__bufferState.allGuids.clear()
                self.__logger.debug("Buffer cleared")

                for obj in message.obj_message:
                    if obj.WhichOneof("message_of_obj") == "character_message":
                        self.__bufferState.allGuids.append(obj.character_message.guid)
                        if obj.character_message.team_id == self.__teamID:
                            self.__bufferState.guids.append(obj.character_message.guid)

                self.__bufferState.gameInfo = Proto2THUAI8.Protobuf2THUAI8GameInfo(
                    message.all_message
                )

                self.__LoadBufferSelf(message)
                if (
                    self.__playerType == THUAI8.PlayerType.Character
                    and self.__bufferState.self is None
                ):
                    self.__logger.debug("exit for null self")
                    return
                for item in message.obj_message:
                    self.__LoadBufferCase(item)
                if Setting.Asynchronous():
                    with self.__mtxState:
                        self.__currentState, self.__bufferState = (
                            self.__bufferState,
                            self.__currentState,
                        )
                        self.__counterState = self.__counterBuffer
                        self.__logger.info("Update state!")
                    self.__freshed = True
                else:
                    self.__bufferUpdated = True
                self.__counterBuffer += 1
                self.__cvBuffer.notify()

        def __LoadBufferSelf(self, message: Message2Clients.MessageToClient) -> None:
            if self.__playerType == THUAI8.PlayerType.Character:
                for item in message.obj_message:
                    if item.WhichOneof("message_of_obj") == "character_message":
                        if (
                            item.character_message.player_id == self.__playerID
                            and item.character_message.team_id == self.__teamID
                        ):
                            self.__bufferState.self = (
                                Proto2THUAI8.Protobuf2THUAI8Character(
                                    item.character_message
                                )
                            )
                            # self.__bufferState.ships.append(self.__bufferState.self)
                            self.__logger.debug("Load self character")
            else:
                for item in message.obj_message:
                    if (
                        item.WhichOneof("message_of_obj") == "team_message"
                        and item.team_message.team_id == self.__teamID
                    ):
                        self.__bufferState.self = Proto2THUAI8.Protobuf2THUAI8Team(
                            item.team_message
                        )
                        self.__logger.debug("Load self team")
                    if item.WhichOneof("message_of_obj") == "character_message":
                        if item.character_message.team_id == self.__teamID:
                            self.__bufferState.characters.append(
                                Proto2THUAI8.Protobuf2THUAI8Character(
                                    item.character_message
                                )
                            )
                            self.__logger.debug("Load Character")

        def __LoadBufferCase(self, item: Message2Clients.MessageOfObj) -> None:
            if self.__playerType == THUAI8.PlayerType.Character:
                if item.WhichOneof("message_of_obj") == "character_message":
                    if item.character_message.team_id != self.__teamID:
                        if AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            item.character_message.x,
                            item.character_message.y,
                            self.__bufferState.gameMap,
                        ):
                            self.__bufferState.enemyCharacters.append(
                                Proto2THUAI8.Protobuf2THUAI8Character(
                                    item.character_message
                                )
                            )
                            self.__logger.debug("Load enemy character")
                    else:
                        self.__bufferState.characters.append(
                            Proto2THUAI8.Protobuf2THUAI8Character(
                                item.character_message
                            )
                        )
                        self.__logger.debug("Load character")
                elif item.WhichOneof("message_of_obj") == "barracks_message":
                    barracks_message = item.barracks_message
                    if (
                        barracks_message.team_id == self.__teamID
                        or AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            barracks_message.x,
                            barracks_message.y,
                            self.__bufferState.gameMap,
                        )
                    ):
                        pos = (
                            AssistFunction.GridToCell(barracks_message.x),
                            AssistFunction.GridToCell(barracks_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.barracksState:
                            self.__bufferState.mapInfo.barracksState[pos] = (
                                barracks_message.team_id,
                                barracks_message.hp,
                            )
                            if barracks_message.team_id == self.__teamID:
                                self.__logger.debug("Load Barracks!")
                            else:
                                self.__logger.debug("Load EnemyBarracks!")
                        else:
                            self.__bufferState.mapInfo.barracksState[pos] = (
                                barracks_message.team_id,
                                barracks_message.hp,
                            )
                            if barracks_message.team_id == self.__teamID:
                                self.__logger.debug("Update Barracks!")
                            else:
                                self.__logger.debug("Update EnemyBarracks!")

                elif item.WhichOneof("message_of_obj") == "spring_message":
                    spring_message = item.spring_message
                    if (
                        spring_message.team_id == self.__teamID
                        or AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            spring_message.x,
                            spring_message.y,
                            self.__bufferState.gameMap,
                        )
                    ):
                        pos = (
                            AssistFunction.GridToCell(spring_message.x),
                            AssistFunction.GridToCell(spring_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.springState:
                            self.__bufferState.mapInfo.springState[pos] = (
                                spring_message.team_id,
                                spring_message.hp,
                            )
                            if spring_message.team_id == self.__teamID:
                                self.__logger.debug("Load Spring!")
                            else:
                                self.__logger.debug("Load EnemySpring!")
                        else:
                            self.__bufferState.mapInfo.springState[pos] = (
                                spring_message.team_id,
                                spring_message.hp,
                            )
                            if spring_message.team_id == self.__teamID:
                                self.__logger.debug("Update Spring!")
                            else:
                                self.__logger.debug("Update EnemySpring!")

                elif item.WhichOneof("message_of_obj") == "farm_message":
                    farm_message = item.farm_message
                    if farm_message.team_id == self.__teamID or AssistFunction.HaveView(
                        self.__bufferState.self.view_range,
                        self.__bufferState.self.x,
                        self.__bufferState.self.y,
                        farm_message.x,
                        farm_message.y,
                        self.__bufferState.gameMap,
                    ):
                        pos = (
                            AssistFunction.GridToCell(farm_message.x),
                            AssistFunction.GridToCell(farm_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.farmState:
                            self.__bufferState.mapInfo.farmState[pos] = (
                                farm_message.team_id,
                                farm_message.hp,
                            )
                            if farm_message.team_id == self.__teamID:
                                self.__logger.debug("Load Farm!")
                            else:
                                self.__logger.debug("Load EnemyFarm!")
                        else:
                            self.__bufferState.mapInfo.farmState[pos] = (
                                farm_message.team_id,
                                farm_message.hp,
                            )
                            if farm_message.team_id == self.__teamID:
                                self.__logger.debug("Update Farm!")
                            else:
                                self.__logger.debug("Update EnemyFarm!")

                elif item.WhichOneof("message_of_obj") == "trap_message":
                    trap_message = item.trap_message
                    if trap_message.team_id == self.__teamID or AssistFunction.HaveView(
                        self.__bufferState.self.view_range,
                        self.__bufferState.self.x,
                        self.__bufferState.self.y,
                        trap_message.x,
                        trap_message.y,
                        self.__bufferState.gameMap,
                    ):
                        pos = (
                            AssistFunction.GridToCell(trap_message.x),
                            AssistFunction.GridToCell(trap_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.trapState:
                            self.__bufferState.mapInfo.trapState[pos] = (
                                trap_message.team_id
                            )
                            if trap_message.team_id == self.__teamID:
                                self.__logger.debug("Load Trap!")
                            else:
                                self.__logger.debug("Load EnemyTrap!")
                        else:
                            self.__bufferState.mapInfo.trapState[pos] = (
                                trap_message.team_id
                            )
                            if trap_message.team_id == self.__teamID:
                                self.__logger.debug("Update Trap!")
                            else:
                                self.__logger.debug("Update EnemyTrap!")

                elif item.WhichOneof("message_of_obj") == "economy_resource_message":
                    economy_message = item.economy_resource_message
                    pos = (
                        AssistFunction.GridToCell(economy_message.x),
                        AssistFunction.GridToCell(economy_message.y),
                    )
                    if pos not in self.__bufferState.mapInfo.economyResourceState:
                        self.__bufferState.mapInfo.economyResourceState[pos] = (
                            economy_message.hp
                        )
                        self.__logger.debug("Load EconomyResource!")
                    else:
                        self.__bufferState.mapInfo.economyResourceState[pos] = (
                            economy_message.hp
                        )
                        self.__logger.debug("Update EconomyResource!")

                elif item.WhichOneof("message_of_obj") == "addition_resource_message":
                    addition_message = item.addition_resource_message
                    pos = (
                        AssistFunction.GridToCell(addition_message.x),
                        AssistFunction.GridToCell(addition_message.y),
                    )
                    if pos not in self.__bufferState.mapInfo.additionResourceState:
                        self.__bufferState.mapInfo.additionResourceState[pos] = (
                            addition_message.hp
                        )
                        self.__logger.debug("Load AdditionResource!")
                    else:
                        self.__bufferState.mapInfo.additionResourceState[pos] = (
                            addition_message.hp
                        )
                        self.__logger.debug("Update AdditionResource!")

                elif item.WhichOneof("message_of_obj") == "news_message":
                    news = item.news_message
                    if news.to_id == self.__playerID and news.team_id == self.__teamID:
                        news_type = Proto2THUAI8.newsTypeDict.get(news.news_case())

                        if news_type == THUAI8.NewsType.TextMessage:
                            self.__messageQueue.append(
                                (news.from_id, news.text_message)
                            )
                            self.__logger.debug("Load Text News!")
                        elif news_type == THUAI8.NewsType.BinaryMessage:
                            self.__messageQueue.append(
                                (news.from_id, news.binary_message)
                            )
                            self.__logger.debug("Load Binary News!")
                        else:
                            self.__logger.error("Unknown NewsType!")
                # NullMessageOfObj和其他默认情况不需要处理

            elif self.__playerType == THUAI8.PlayerType.Team:
                # 定义视野检查函数
                def HaveOverView(targetX: int, targetY: int) -> bool:
                    for character in self.__bufferState.characters:
                        if AssistFunction.HaveView(
                            character.x,
                            character.y,
                            targetX,
                            targetY,
                            character.viewRange,
                            self.__bufferState.gameMap,
                        ):
                            return True
                    return False

                def HaveOverTrapView(targetX: int, targetY: int) -> bool:
                    for character in self.__bufferState.characters:
                        if (
                            AssistFunction.HaveView(
                                character.x,
                                character.y,
                                targetX,
                                targetY,
                                character.viewRange,
                                self.__bufferState.gameMap,
                            )
                            and character.visionBuffTime > 0
                        ):
                            return True
                    return False

                if item.WhichOneof("message_of_obj") == "character_message":
                    if item.character_message.team_id != self.__teamID:
                        if AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            item.character_message.x,
                            item.character_message.y,
                            self.__bufferState.gameMap,
                        ):
                            self.__bufferState.enemyCharacters.append(
                                Proto2THUAI8.Protobuf2THUAI8Character(
                                    item.character_message
                                )
                            )
                            self.__logger.debug("Load enemy character")
                    else:
                        self.__bufferState.characters.append(
                            Proto2THUAI8.Protobuf2THUAI8Character(
                                item.character_message
                            )
                        )
                        self.__logger.debug("Load character")
                elif item.WhichOneof("message_of_obj") == "barracks_message":
                    barracks_message = item.barracks_message
                    if (
                        barracks_message.team_id == self.__teamID
                        or AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            barracks_message.x,
                            barracks_message.y,
                            self.__bufferState.gameMap,
                        )
                    ):
                        pos = (
                            AssistFunction.GridToCell(barracks_message.x),
                            AssistFunction.GridToCell(barracks_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.barracksState:
                            self.__bufferState.mapInfo.barracksState[pos] = (
                                barracks_message.team_id,
                                barracks_message.hp,
                            )
                            if barracks_message.team_id == self.__teamID:
                                self.__logger.debug("Load Barracks!")
                            else:
                                self.__logger.debug("Load EnemyBarracks!")
                        else:
                            self.__bufferState.mapInfo.barracksState[pos] = (
                                barracks_message.team_id,
                                barracks_message.hp,
                            )
                            if barracks_message.team_id == self.__teamID:
                                self.__logger.debug("Update Barracks!")
                            else:
                                self.__logger.debug("Update EnemyBarracks!")

                elif item.WhichOneof("message_of_obj") == "spring_message":
                    spring_message = item.spring_message
                    if (
                        spring_message.team_id == self.__teamID
                        or AssistFunction.HaveView(
                            self.__bufferState.self.view_range,
                            self.__bufferState.self.x,
                            self.__bufferState.self.y,
                            spring_message.x,
                            spring_message.y,
                            self.__bufferState.gameMap,
                        )
                    ):
                        pos = (
                            AssistFunction.GridToCell(spring_message.x),
                            AssistFunction.GridToCell(spring_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.springState:
                            self.__bufferState.mapInfo.springState[pos] = (
                                spring_message.team_id,
                                spring_message.hp,
                            )
                            if spring_message.team_id == self.__teamID:
                                self.__logger.debug("Load Spring!")
                            else:
                                self.__logger.debug("Load EnemySpring!")
                        else:
                            self.__bufferState.mapInfo.springState[pos] = (
                                spring_message.team_id,
                                spring_message.hp,
                            )
                            if spring_message.team_id == self.__teamID:
                                self.__logger.debug("Update Spring!")
                            else:
                                self.__logger.debug("Update EnemySpring!")

                elif item.WhichOneof("message_of_obj") == "farm_message":
                    farm_message = item.farm_message
                    if farm_message.team_id == self.__teamID or AssistFunction.HaveView(
                        self.__bufferState.self.view_range,
                        self.__bufferState.self.x,
                        self.__bufferState.self.y,
                        farm_message.x,
                        farm_message.y,
                        self.__bufferState.gameMap,
                    ):
                        pos = (
                            AssistFunction.GridToCell(farm_message.x),
                            AssistFunction.GridToCell(farm_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.farmState:
                            self.__bufferState.mapInfo.farmState[pos] = (
                                farm_message.team_id,
                                farm_message.hp,
                            )
                            if farm_message.team_id == self.__teamID:
                                self.__logger.debug("Load Farm!")
                            else:
                                self.__logger.debug("Load EnemyFarm!")
                        else:
                            self.__bufferState.mapInfo.farmState[pos] = (
                                farm_message.team_id,
                                farm_message.hp,
                            )
                            if farm_message.team_id == self.__teamID:
                                self.__logger.debug("Update Farm!")
                            else:
                                self.__logger.debug("Update EnemyFarm!")

                elif item.WhichOneof("message_of_obj") == "trap_message":
                    trap_message = item.trap_message
                    if trap_message.team_id == self.__teamID or AssistFunction.HaveView(
                        self.__bufferState.self.view_range,
                        self.__bufferState.self.x,
                        self.__bufferState.self.y,
                        trap_message.x,
                        trap_message.y,
                        self.__bufferState.gameMap,
                    ):
                        pos = (
                            AssistFunction.GridToCell(trap_message.x),
                            AssistFunction.GridToCell(trap_message.y),
                        )
                        if pos not in self.__bufferState.mapInfo.trapState:
                            self.__bufferState.mapInfo.trapState[pos] = (
                                trap_message.team_id
                            )
                            if trap_message.team_id == self.__teamID:
                                self.__logger.debug("Load Trap!")
                            else:
                                self.__logger.debug("Load EnemyTrap!")
                        else:
                            self.__bufferState.mapInfo.trapState[pos] = (
                                trap_message.team_id
                            )
                            if trap_message.team_id == self.__teamID:
                                self.__logger.debug("Update Trap!")
                            else:
                                self.__logger.debug("Update EnemyTrap!")

                elif item.WhichOneof("message_of_obj") == "economy_resource_message":
                    economy_message = item.economy_resource_message
                    pos = (
                        AssistFunction.GridToCell(economy_message.x),
                        AssistFunction.GridToCell(economy_message.y),
                    )
                    if pos not in self.__bufferState.mapInfo.economyResourceState:
                        self.__bufferState.mapInfo.economyResourceState[pos] = (
                            economy_message.hp
                        )
                        self.__logger.debug("Load EconomyResource!")
                    else:
                        self.__bufferState.mapInfo.economyResourceState[pos] = (
                            economy_message.hp
                        )
                        self.__logger.debug("Update EconomyResource!")

                elif item.WhichOneof("message_of_obj") == "addition_resource_message":
                    addition_message = item.addition_resource_message
                    pos = (
                        AssistFunction.GridToCell(addition_message.x),
                        AssistFunction.GridToCell(addition_message.y),
                    )
                    if pos not in self.__bufferState.mapInfo.additionResourceState:
                        self.__bufferState.mapInfo.additionResourceState[pos] = (
                            addition_message.hp
                        )
                        self.__logger.debug("Load AdditionResource!")
                    else:
                        self.__bufferState.mapInfo.additionResourceState[pos] = (
                            addition_message.hp
                        )
                        self.__logger.debug("Update AdditionResource!")

                elif item.WhichOneof("message_of_obj") == "news_message":
                    news = item.news_message
                    if news.to_id == self.__playerID and news.team_id == self.__teamID:
                        news_type = Proto2THUAI8.newsTypeDict.get(news.news_case())

                        if news_type == THUAI8.NewsType.TextMessage:
                            self.__messageQueue.append(
                                (news.from_id, news.text_message)
                            )
                            self.__logger.debug("Load Text News!")
                        elif news_type == THUAI8.NewsType.BinaryMessage:
                            self.__messageQueue.append(
                                (news.from_id, news.binary_message)
                            )
                            self.__logger.debug("Load Binary News!")
                        else:
                            self.__logger.error("Unknown NewsType!")
                # NullMessageOfObj和其他默认情况不需要处理
