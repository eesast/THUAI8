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
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;

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
        public long AddCharacter(PlayerInitInfo playerInitInfo)
        {
            if (teamList[(int)playerInitInfo.teamID].CharacterNum >= GameData.CharacterTotalNumMax)
            {
                GameLogging.logger.LogDebug($"Failed to add character: team {playerInitInfo.teamID} reached max character limit");
                return GameObj.invalidID;
            }
            teamList[(int)playerInitInfo.teamID].CharacterNum.Add(1);
            if (!gameMap.TeamExists(playerInitInfo.teamID))
            {
                GameLogging.logger.LogDebug($"Failed to add character: team {playerInitInfo.teamID} does not exist");
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
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: invalid character type Null");
                                return GameObj.invalidID;
                            }
                        case CharacterType.TangSeng:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.TangSeng)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max TangSeng limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.SunWukong:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.SunWukong)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max SunWukong limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ZhuBajie:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ZhuBajie)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max ZhuBajie limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ShaWujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ShaWujing)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max ShaWujing limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.BaiLongma:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.BaiLongma)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max BaiLongma limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.Monkid:
                            break;
                        default:
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: invalid character type {characterType}");
                                return GameObj.invalidID;
                            }
                    }
                }
                else
                {
                    switch (characterType)
                    {
                        case CharacterType.Null:
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: invalid character type Null");
                                return GameObj.invalidID;
                            }
                        case CharacterType.JiuLing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.JiuLing)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max JiuLing limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.HongHaier:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.HongHaier)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max HongHaier limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.NiuMowang:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.NiuMowang)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max NiuMowang limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.TieShan:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.TieShan)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max TieShan limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.ZhiZhujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.ZhiZhujing)
                                >= GameData.MaxCharacterNum)
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: reached max ZhiZhujing limit");
                                return GameObj.invalidID;
                            }
                            break;
                        case CharacterType.Pawn:
                            break;
                        default:
                            {
                                GameLogging.logger.LogDebug($"Failed to add character: invalid character type {characterType}");
                                return GameObj.invalidID;
                            }
                    }
                }
                Character? newCharacter = CharacterManager.AddCharacter(playerInitInfo.teamID,
                                                    playerInitInfo.playerID,
                                                    playerInitInfo.characterType,
                                                    teamList[(int)playerInitInfo.teamID].MoneyPool);
                if (newCharacter == null)
                {
                    GameLogging.logger.LogDebug($"Failed to add character: character manager returned null");
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
        public bool Recycle(long teamID, long characterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null)
            {
                if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                {
                    return false;
                }
                if (teamList[(int)teamID].sideFlag == 0)
                {
                    if (character.CharacterType >= CharacterType.JiuLing)
                    {
                        return false;
                    }
                }
                if (teamList[(int)teamID].sideFlag == 1)
                {
                    if (character.CharacterType < CharacterType.JiuLing)
                    {
                        return false;
                    }
                }
            }

            if (character != null && character.IsRemoved == false)
            {
                bool validRecyclePoint = false;
                foreach (XY recyclePoint in teamList[(int)character.TeamID].BirthPointList)
                {
                    if (GameData.ApproachToInteract(character.Position, recyclePoint) && character.Position != recyclePoint)
                    {
                        validRecyclePoint = true;
                        break;
                    }
                }
                if (validRecyclePoint)
                {
                    return characterManager.Recycle(character);
                }
            }
            return false;
        }
        public long ActivateCharacter(long teamID, CharacterType characterType, int birthPointIndex = 0)
        {
            GameLogging.logger.LogDebug($"Try to activate {teamID} {characterType} at birthpoint {birthPointIndex}");
            Character? character = teamList[(int)teamID].CharacterPool.GetObj(characterType);
            if (character == null)
            {
                GameLogging.logger.LogDebug($"Fail to activate {teamID} {characterType}, no character available");
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
                GameLogging.logger.LogDebug($"Successfully activated {teamID} {characterType} at {pos}");
                return character.PlayerID;
            }
            else
            {
                teamList[(int)teamID].CharacterPool.ReturnObj(character);
                GameLogging.logger.LogDebug($"Fail to activate {teamID} {characterType} at {pos}, rule not permitted");
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
            ClearAllLists();
            gameMap.Timer.EndGame();
        }
        public bool MoveCharacter(long teamID, long characterID, int moveTimeInMilliseconds, double angle)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
            {
                GameLogging.logger.LogDebug(
                    "Try to move "
                    + LoggingFunctional.CharacterLogInfo(character)
                    + $" {moveTimeInMilliseconds} {angle}");
                return actionManager.MoveCharacter(character, moveTimeInMilliseconds, angle);
            }
            else
            {
                GameLogging.logger.LogDebug(
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
        public void AddHoleTrap(long teamID, XY pos)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            if (teamList[(int)teamID].HoleList.Contains(pos))
                return;
            teamList[(int)teamID].HoleList.Add(pos);
        }
        public void RemoveHoleTrap(long teamID, XY pos)
        {
            if (!gameMap.TeamExists(teamID))
                return;
            if (!teamList[(int)teamID].HoleList.Contains(pos))
                return;
            teamList[(int)teamID].HoleList.Remove(pos);
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
                    gameMap.GameObjDict[GameObjType.CHARACTER].ForEach(delegate (IGameObj character)
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
            ARManager = new(this, gameMap, characterManager);
            actionManager = new(this, gameMap, characterManager);
            attackManager = new(this, gameMap, characterManager, ARManager);
            skillCastManager = new(this, gameMap, characterManager, ARManager, actionManager);
            equipManager = new();
            teamList = [];
            gameMap.GameObjDict[GameObjType.HOME].Cast<GameObj>()?.ForEach(
                delegate (GameObj gameObj)
                {
                    if (gameObj.Type == GameObjType.HOME)
                    {
                        teamList.Add(new Base((Home)gameObj));
                        teamList.Last().BirthPointList.Add(gameObj.Position);
                        teamList.Last().AddMoney(GameData.InitialMoney);
                    }
                }
            );
        }
        public bool Recover(long teamID, long characterID, long recover)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
            {
                bool validRecoverPoint = false;
                foreach (XY recoverPoint in teamList[(int)character.TeamID].BirthPointList)
                {
                    if (GameData.ApproachToInteract(character.Position, recoverPoint) && character.Position != recoverPoint)
                    {
                        validRecoverPoint = true;
                        break;
                    }
                }
                if (validRecoverPoint)
                {
                    return characterManager.Recover(character, recover);
                }
            }
            return false;
        }
        public bool Attack(long teamID, long characterID, long ATKteamID, long ATKcharacterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            Character? ObjBeingShot = gameMap.FindCharacterInPlayerID(ATKteamID, ATKcharacterID);
            if (character != null && character.IsRemoved == false)
                return attackManager.Attack(character, ObjBeingShot);
            return false;
        }
        public bool Construct(long teamID, long characterID, ConstructionType constructionType)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false && constructionType == ConstructionType.CAGE)
            {
                return actionManager.SetTrap(character, TrapType.CAGE);
            }
            if (character != null && character.IsRemoved == false && constructionType == ConstructionType.HOLE)
            {
                return actionManager.SetTrap(character, TrapType.HOLE);
            }
            if (character != null && character.IsRemoved == false)
            {
                return actionManager.Construct(character, constructionType);
            }
            return false;
        }
        public bool AttackConstruction(long teamID, long characterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            Construction? construction = (Construction?)gameMap.OneForInteract(character.Position, GameObjType.CONSTRUCTION);
            if (character != null && character.IsRemoved == false && construction != null)
            {
                return attackManager.Attack(character, construction);
            }
            return false;
        }
        public bool Produce(long teamID, long characterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
                return actionManager.Produce(character);
            return false;
        }
        public bool CastSkill(long teamID, long characterID, double angle = 0.0)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
                return skillCastManager.SkillCasting(character, angle);
            return false;
        }
        public bool Equip(long teamID, long characterID, EquipmentType equiptype)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            int nowtime = gameMap.Timer.NowTime();
            if (nowtime >= GameData.SevenMinutes)
            {
                if (equiptype == EquipmentType.INVISIBILITY_POTION)
                {
                    return false;
                }
            }
            else if (equiptype == EquipmentType.BERSERK_POTION)
            {
                return false;
            }
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null && character.IsRemoved == false)
                return equipManager.GetEquipment(character, equiptype);
            return false;
        }
        public bool Stop(long teamID, long characterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null)
                return ActionManager.Stop(character);
            return false;
        }
        public bool AttackResource(long teamID, long characterID)
        {
            if (!gameMap.Timer.IsGaming)
                return false;
            Character? character = gameMap.FindCharacterInPlayerID(teamID, characterID);
            if (character != null)
                return attackManager.AttackResource(character);
            return false;
        }
    }
}