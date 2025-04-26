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
    def BuddhistsCharacterTypes() -> List[THUAI8.CharacterType]:
        return [
            THUAI8.CharacterType.TangSeng,
            THUAI8.CharacterType.SunWukong,
            THUAI8.CharacterType.ZhuBajie,
            THUAI8.CharacterType.ShaWujing,
            THUAI8.CharacterType.BaiLongma,
            THUAI8.CharacterType.Monkid,
        ]

    @staticmethod
    def MonsterCharacterTypes() -> List[THUAI8.CharacterType]:
        return [
            THUAI8.CharacterType.JiuLing,
            THUAI8.CharacterType.HongHaier,
            THUAI8.CharacterType.NiuMowang,
            THUAI8.CharacterType.TieShan,
            THUAI8.CharacterType.ZhiZhujing,
            THUAI8.CharacterType.Pawn,
        ]


numOfGridPerCell: Final[int] = 1000


class AI(IAI):
    def __init__(self, pID: int):
        self.__playerID = pID

    def CharacterPlay(self, api: ICharacterAPI) -> None:
        # 公共操作
        if self.__playerID == 1:
            # player1的操作
            api.PrintSelfInfo()
            api.Move(100, 100, 0)
            return
        elif self.__playerID == 2:
            # player2的操作
            api.Move(2500, 1000, 0)
            return
        elif self.__playerID == 3:
            # player3的操作
            return
        elif self.__playerID == 4:
            # player4的操作
            return
        return

    def TeamPlay(self, api: ITeamAPI) -> None:
        # player0的操作
        api.PrintSelfInfo()
        return
