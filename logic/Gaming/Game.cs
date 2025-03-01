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
    }
}