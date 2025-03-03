using GameClass.GameObj;
using GameClass.GameObj.Map;
using GameClass.GameObj.Areas;
using GameClass.MapGenerator;
using GameClass.GameObj.Occupations;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gaming
{
    public partial class Game
    {
        public struct PlayerInitInfo(long teamID, long playerID, CharacterType characterType, int sideFlag)//sideFlag表示队伍是取经阵营还是妖怪阵营
        {
            public long teamID = teamID;
            public long playerID = playerID;
            public CharacterType characterType = characterType;
            public int sideFlag = sideFlag;//0是取经队，1是妖怪队
        }
        private readonly List<Base> teamList;
        public List<Base> TeamList => teamList;
        private readonly Map gameMap;
        public Map GameMap => gameMap;
        private readonly Random random = new();
        public long AddPlayer(PlayerInitInfo playerInitInfo)
        {
            if (gameMap.TeamExists(playerInitInfo.teamID))
            {
                return GameObj.invalidID;
            }
            if (playerInitInfo.playerID != 0)
            {
                var characterType = playerInitInfo.characterType;
                if (playerInitInfo.sideFlag == 0)
                {
                    switch (characterType)
                    {
                        case CharacterType.Null:
                            return GameObj.invalidID;
                        case CharacterType.TangSeng:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.TangSeng)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.SunWukong:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.SunWukong)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ZhuBajie:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ZhuBajie)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ShaWujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ShaWujing)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.BaiLongma:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.BaiLongma)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.Monkid:
                            break;
                        default:
                            return GameObj.invalidID;
                    }
                }
                else
                {
                    switch (characterType)
                    {
                        case CharacterType.Null:
                            return GameObj.invalidID;
                        case CharacterType.JiuLing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.JiuLing)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.HongHaier:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.HongHaier)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.NiuMowang:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.NiuMowang)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.TieShan:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.TieShan)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ZhiZhujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ZhiZhujing)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.Pawn:
                            break;
                        default:
                            return GameObj.invalidID;
                    }
                }
                Character? newCharacter = CharacterManager.AddCharacter(playerInitInfo.teamID,
                                                    playerInitInfo.playerID,
                                                    playerInitInfo.characterType,
                                                    teamList[(int)playerInitInfo.teamID].MoneyPool);
                if (newCharacter == null)
                {
                    return GameObj.invalidID;
                }
                teamList[(int)playerInitInfo.teamID].CharacterPool.Append(newCharacter);
                return newCharacter.PlayerID;
            }
            else
            {
                return playerInitInfo.playerID;
            }
        }
        public long ActivateCharacter(long teamID, CharacterType characterType, int birthPointIndex = 0)
        {
            GameLogging.logger.ConsoleLogDebug($"Try to activate {teamID} {characterType} at birthpoint {birthPointIndex}");
            Character? character = teamList[(int)teamID].CharacterPool.GetObj(characterType);
            if (character == null)
            {
                GameLogging.logger.ConsoleLogDebug($"Fail to activate {teamID} {characterType}, no character available");
                return GameObj.invalidID;
            }
            if (birthPointIndex < 0)
                birthPointIndex = 0;
            if (birthPointIndex >= teamList[(int)teamID].BirthPointList.Count)
                birthPointIndex = teamList[(int)teamID].BirthPointList.Count - 1;
            XY pos = teamList[(int)teamID].BirthPointList[birthPointIndex];
            pos += new XY(((random.Next() & 2) - 1) * 1000, ((random.Next() & 2) - 1) * 1000);
            if (characterManager.ActivateCharacter(character, pos))
            {
                GameLogging.logger.ConsoleLogDebug($"Successfully activated {teamID} {characterType} at {pos}");
                return character.PlayerID;
            }
            else
            {
                teamList[(int)teamID].CharacterPool.ReturnObj(character);
                GameLogging.logger.ConsoleLogDebug($"Fail to activate {teamID} {characterType} at {pos}, rule not permitted");
                return GameObj.invalidID;
            }
        }
        public bool StartGame(int milliSeconds)
        {
            if (gameMap.Timer.IsGaming)
                return false;
            // 开始游戏
            foreach (var team in TeamList)
            {
                actionManager.TeamTask(team);
                if (team.sideFlag == 0)
                {
                    ActivateCharacter(team.TeamID, CharacterType.TangSeng);
                }
                else
                {
                    ActivateCharacter(team.TeamID, CharacterType.JiuLing);
                }
            }
            gameMap.Timer.Start(() => { }, () => EndGame(), milliSeconds);
            return true;
        }
        public void EndGame()
        {
        }
        public bool MoveCharacter(long teamID, long characterID, int moveTimeInMilliseconds, double angle)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
            {
                GameLogging.logger.ConsoleLogDebug(
                    "Try to move "
                    + LoggingFunctional.CharacterLogInfo(character)
                    + $" {moveTimeInMilliseconds} {angle}");
                return actionManager.MoveCharacter(character, moveTimeInMilliseconds, angle);
            }
            else
            {
                GameLogging.logger.ConsoleLogDebug(
                    "Fail to move "
                    + LoggingFunctional.CharacterLogInfo(teamID, characterID)
                    + ", not found");
                return false;
            }
        }
        public void AddBirthPoint(long teamID, XY pos)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            if (teamList[(int)teamID].BirthPointList.Contains(pos))
                return;
            teamList[(int)teamID].BirthPointList.Add(pos);
        }
        public void RemoveBirthPoint(long teamID, XY pos)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            if (!teamList[(int)teamID].BirthPointList.Contains(pos))
                return;
            teamList[(int)teamID].BirthPointList.Remove(pos);
        }
        public void AddFactory(long teamID)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            teamList[(int)teamID].FarmNum.Add(1);
        }
        public void RemoveFactory(long teamID)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            teamList[(int)teamID].FarmNum.Sub(1);
        }
        public long GetTeamMoney(long teamID)
        {
            if (!gameMap.TeamExists(teamID))
                return -1;
            return teamList[(int)teamID].MoneyPool.Money;
        }
        public long GetTeamScore(long teamID)
        {
            if (!gameMap.TeamExists(teamID))
                return -1;
            return teamList[(int)teamID].MoneyPool.Score;
        }
        public List<IGameObj> GetGameObj()
        {
            var gameObjList = new List<IGameObj>();
            foreach (var keyValuePair in gameMap.GameObjDict)
            {
                if (GameData.NeedCopy(keyValuePair.Key))
                {
                    var thisList = gameMap.GameObjDict[keyValuePair.Key].ToNewList();
                    if (thisList != null) gameObjList.AddRange(thisList);
                }
            }
            return gameObjList;
        }
        public void ClearAllLists()
        {
            foreach (var keyValuePair in gameMap.GameObjDict)
            {
                if (!GameData.NeedCopy(keyValuePair.Key))
                {
                    gameMap.GameObjDict[GameObjType.Character].ForEach(delegate (IGameObj character)
                    {
                        ((Character)character).CanMove.SetROri(false);
                    });
                    gameMap.GameObjDict[keyValuePair.Key].Clear();
                }
            }
        }
        public Game(MapStruct mapResource, int numOfTeam)
        {
            gameMap = new(mapResource);
            characterManager = new(this, gameMap);
            skillCastManager = new();
            actionManager = new(this, gameMap, characterManager);
            attackManager = new(this, gameMap, characterManager);
            ARManager = new(this, gameMap);
            teamList = [];
            gameMap.GameObjDict[GameObjType.Home].Cast<GameObj>()?.ForEach(
                delegate (GameObj gameObj)
                {
                    if (gameObj.Type == GameObjType.Home)
                    {
                        teamList.Add(new Base((Home)gameObj));
                        teamList.Last().BirthPointList.Add(gameObj.Position);
                        teamList.Last().AddMoney(GameData.InitialMoney);
                    }
                }
            );
        }
    }
}