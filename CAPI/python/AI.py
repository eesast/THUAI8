import queue
import time
from typing import Final, List, Union, cast

import PyAPI.structures as THUAI8
from PyAPI.constants import Constants
from PyAPI.Interface import IAI, ICharacterAPI, ITeamAPI
from PyAPI.utils import AssistFunction


class Setting:
    # 为假则play()期间确保游戏状态不更新，为真则只保证游戏状态在调用相关方法时不更新，大致一帧更新一次
    @staticmethod
    def Asynchronous() -> bool:
        return False

    @staticmethod
<<<<<<< HEAD
    def BuddhistsCharacterTypes() -> List[THUAI8.CharacterType]:
        return [
            THUAI8.CharacterType.Monk,
            THUAI8.CharacterType.MonkyKing,
            THUAI8.CharacterType.Pigsy,
            THUAI8.CharacterType.ShaWujing,
            THUAI8.CharacterType.Whitedragonhorse,
        ]

    @staticmethod
    def MonsterCharacterTypes() -> List[THUAI8.CharacterType]:
        return [
            THUAI8.CharacterType.JiuTouYuanSheng,
            THUAI8.CharacterType.HongHaier,
=======
    def ShipTypes() -> List[THUAI8.CharacterType]:
        return [
            THUAI8.CharacterType.Monk,
            THUAI8.CharacterType.MonkeyKing,
            THUAI8.CharacterType.Pigsy,
            THUAI8.CharacterType.ShaWujing,
            THUAI8.CharacterType.Whitedragonhorse,
            THUAI8.CharacterType.JiuTouYuanSheng,
            THUAI8.CharacterType.Honghaier,
>>>>>>> 5a1751dc64f09ba19aaa818e5dc99172d15f4c69
            THUAI8.CharacterType.Gyuumao,
            THUAI8.CharacterType.Princess_Iron_Fan,
            THUAI8.CharacterType.Spider,
        ]
<<<<<<< HEAD


numOfGridPerCell: Final[int] = 1000


=======
numOfGridPerCell: Final[int] = 1000

>>>>>>> 5a1751dc64f09ba19aaa818e5dc99172d15f4c69
class AI(IAI):
    def __init__(self, pID: int):
        self.__playerID = pID

<<<<<<< HEAD
    def CharacterPlay(self, api: ICharacterAPI) -> None:
=======
    def ShipPlay(self, api: IShipAPI) -> None:
>>>>>>> 5a1751dc64f09ba19aaa818e5dc99172d15f4c69
        # 公共操作
        if self.__playerID == 1:
            # player1的操作
            return
        elif self.__playerID == 2:
            # player2的操作
            return
        elif self.__playerID == 3:
            # player3的操作
            return
        elif self.__playerID == 4:
            # player4的操作
            return
<<<<<<< HEAD
        return
=======
        elif self.__playerID == 5:
            # player4的操作
            return
>>>>>>> 5a1751dc64f09ba19aaa818e5dc99172d15f4c69

    def TeamPlay(self, api: ITeamAPI) -> None:
        # player0的操作
        return
