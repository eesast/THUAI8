import threading
import time
from typing import Union

import grpc
import proto.Message2Clients_pb2 as Message2Clients
import proto.Services_pb2_grpc as Services
import PyAPI.structures as THUAI8
from PyAPI.AI import Setting
from PyAPI.Interface import IErrorHandler
from PyAPI.utils import THUAI82Proto


class BoolErrorHandler(IErrorHandler):
    @staticmethod
    def result():
        return False


class Communication:
    def __init__(self, sIP: str, sPort: str):
        aim = sIP + ":" + sPort
        channel = grpc.insecure_channel(aim)
        self.__THUAI8Stub = Services.AvailableServiceStub(channel)
        self.__haveNewMessage = False
        self.__cvMessage = threading.Condition()
        self.__message2Client: Message2Clients.MessageToClient
        self.__mtxLimit = threading.Lock()
        self.__counter = 0
        self.__counterMove = 0
        self.__limit = 50
        self.__moveLimit = 10

    def Move(
        self, teamID: int, playerID: int, timeInMiliseconds: int, angle: float
    ) -> bool:
        try:
            with self.__mtxLimit:
                if (
                    self.__counter >= self.__limit
                    or self.__counterMove >= self.__moveLimit
                ):
                    return False
                self.__counter += 1
                self.__counterMove += 1
            moveResult: Message2Clients.MoveRes = self.__THUAI8Stub.Move(
                THUAI82Proto.THUAI82ProtobufMoveMsg(
                    teamID, playerID, timeInMiliseconds, angle
                )
            )
        except grpc.RpcError:
            return False
        else:
            return moveResult.act_success

    def Send(
        self, toID: int, message: Union[str, bytes], playerID: int, teamID: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            sendResult: Message2Clients.BoolRes = self.__THUAI8Stub.Send(
                THUAI82Proto.THUAI82ProtobufSendMsg(
                    playerID,
                    toID,
                    teamID,
                    message,
                    True if isinstance(message, bytes) else False,
                )
            )
        except grpc.RpcError:
            return False
        else:
            return sendResult.act_success

    def Common_Attack(
        self, playerID: int, teamID: int, attackedPlayerID: int, attackedTeamID: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            commonAttackResult: Message2Clients.BoolRes = self.__THUAI8Stub.Attack(
                THUAI82Proto.THUAI82ProtobufAttackMsg(
                    playerID, teamID, attackedPlayerID, attackedTeamID
                )
            )
        except grpc.RpcError:
            return False
        else:
            return commonAttackResult.act_success()

    def Skill_Attack(self, playerID: int, teamID: int, angle: float) -> bool:
        self.__skillrange = 0  # 技能范围待修改,技能位置待定
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            skillAttackResult: Message2Clients.BoolRes = self.__THUAI8Stub.Cast(
                THUAI82Proto.THUAI82ProtobufCastMsg(
                    playerID, 0, teamID, self.__skillrange, 0, 0, angle
                )
            )
        except grpc.RpcError:
            return False
        else:
            return skillAttackResult.act_success()

    def Recover(self, playerID: int, teamID: int, recover: int) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            recoverResult: Message2Clients.BoolRes = self.__THUAI8Stub.Recover(
                THUAI82Proto.THUAI82ProtobufRecoverMsg(playerID, teamID, recover)
            )
        except grpc.RpcError:
            return False
        else:
            return recoverResult.act_success

    def Produce(self, playerID: int, teamID: int) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            produceResult: Message2Clients.BoolRes = self.__THUAI8Stub.Produce(
                THUAI82Proto.THUAI82ProtobufIDMsg(playerID, teamID)
            )
        except grpc.RpcError:
            return False
        else:
            return produceResult.act_success

    def Rebuild(
        self, constructionType: THUAI8.ConstructionType, playerID: int, teamID: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            rebuildResult: Message2Clients.BoolRes = self.__THUAI8Stub.Rebuild(
                THUAI82Proto.THUAI82ProtobufConstructMsg(
                    playerID, teamID, constructionType
                )
            )
        except grpc.RpcError:
            return False
        else:
            return rebuildResult.act_success

    def Construct(
        self, constructionType: THUAI8.ConstructionType, playerID: int, teamID: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            constructResult: Message2Clients.BoolRes = self.__THUAI8Stub.Construct(
                THUAI82Proto.THUAI82ProtobufConstructMsg(
                    playerID, teamID, constructionType
                )
            )
        except grpc.RpcError:
            return False
        else:
            return constructResult.act_success

    def InstallEquipment(
        self, equipmentType: THUAI8.EquipmentType, playerID: int, teamID: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            installEquipmentResult: Message2Clients.BoolRes = self.__THUAI8Stub.Equip(
                THUAI82Proto.THUAI82ProtobufEquipMsg(playerID, teamID, equipmentType)
            )
        except grpc.RpcError:
            return False
        else:
            return installEquipmentResult.act_success

    def EndAllAction(self, playerID: int, teamID: int) -> bool:
        try:
            with self.__mtxLimit:
                if (
                    self.__counter >= self.__limit
                    or self.__counterMove >= self.__moveLimit
                ):
                    return False
                self.__counter += 1
                self.__counterMove += 1
            endResult: Message2Clients.BoolRes = self.__THUAI8Stub.EndAllAction(
                THUAI82Proto.THUAI82ProtobufIDMsg(playerID, teamID)
            )
        except grpc.RpcError:
            return False
        else:
            return endResult.act_success

    def Recycle(self, playerID: int, teamID: int) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            recycleResult: Message2Clients.BoolRes = self.__THUAI8Stub.Recycle(
                THUAI82Proto.THUAI82ProtobufIDMsg(playerID, teamID)
            )
        except grpc.RpcError:
            return False
        else:
            return recycleResult.act_success

    def BuildCharacter(
        self, teamID: int, characterType: THUAI8.CharacterType, birthIndex: int
    ) -> bool:
        try:
            with self.__mtxLimit:
                if self.__counter >= self.__limit:
                    return False
                self.__counter += 1
            buildResult: Message2Clients.BoolRes = self.__THUAI8Stub.CreatCharacter(
                THUAI82Proto.THUAI82ProtobufCreatCharacterMsg(
                    teamID, characterType, birthIndex
                )
            )
        except grpc.RpcError:
            return False
        else:
            return buildResult.act_success

    def TryConnection(self, playerID: int, teamID: int) -> bool:
        try:
            tryResult: Message2Clients.BoolRes = self.__THUAI8Stub.TryConnection(
                THUAI82Proto.THUAI82ProtobufIDMsg(playerID, teamID)
            )
        except grpc.RpcError:
            return False
        else:
            return tryResult.act_success

    def GetMessage2Client(self) -> Message2Clients.MessageToClient:
        with self.__cvMessage:
            self.__cvMessage.wait_for(lambda: self.__haveNewMessage)
            self.__haveNewMessage = False
            return self.__message2Client

    def AddPlayer(
        self,
        playerID: int,
        teamID: int,
        characterType: THUAI8.CharacterType,
        side_flag: bool,
    ) -> None:
        def tMessage():
            try:
                if playerID == 0:
                    playerMsg = THUAI82Proto.THUAI82ProtobufCharacterMsg(
                        playerID, teamID, characterType, side_flag
                    )
                    for msg in self.__THUAI8Stub.AddCharacter(playerMsg):
                        with self.__cvMessage:
                            self.__haveNewMessage = True
                            self.__message2Client = msg
                            self.__cvMessage.notify()
                            with self.__mtxLimit:
                                self.__counter = 0
                                self.__counterMove = 0
                elif playerID >= 1 and playerID <= 6:
                    playerMsg = THUAI82Proto.THUAI82ProtobufCharacterMsg(
                        playerID, teamID, characterType, side_flag
                    )
                    for msg in self.__THUAI8Stub.AddCharacter(playerMsg):
                        with self.__cvMessage:
                            self.__haveNewMessage = True
                            self.__message2Client = msg
                            self.__cvMessage.notify()
                            with self.__mtxLimit:
                                self.__counter = 0
                                self.__counterMove = 0
            except grpc.RpcError:
                return

        threading.Thread(target=tMessage).start()
