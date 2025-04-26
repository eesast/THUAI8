from typing import List, Union

import PyAPI.structures as THUAI8


class State:
    def __init__(self) -> None:
        self.teamScore = 0
        self.self = None
        self.characters = []
        self.enemyCharacters = []
        self.gameMap = []
        self.mapInfo = THUAI8.GameMap()
        self.gameInfo = THUAI8.GameInfo()
        self.guids = []
        self.allGuids = []

    teamScore: int
    self: Union[THUAI8.Character, THUAI8.Team]

    characters: List[THUAI8.Character]
    enemyCharacters: List[THUAI8.Character]

    gameMap: List[List[THUAI8.PlaceType]]

    mapInfo: THUAI8.GameMap

    gameInfo: THUAI8.GameInfo

    guids: List[int]
    allGuids: List[int]
