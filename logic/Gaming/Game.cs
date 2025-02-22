using GameClass.GameObj;
using GameClass.GameObj.Map;
using GameClass.GameObj.Areas;
using GameClass.MapGenerator;
using GameClass.Occupations;
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
        public struct PlayerInitInfo(long teamID, long playerID, CharacterType characterType, bool sideFlag)//sideFlag表示队伍是取经阵营还是妖怪阵营
        {
            public long teamID = teamID;
            public long playerID = playerID;
            public CharacterType characterType = characterType;
            public bool sideFlag = sideFlag;//0是取经队，1是妖怪队
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
                        case characterType.Null:
                            return GameObj.invalidID;
                        case characterType.TangSeng:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Tangseng)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.SunWukong:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Sunwukong)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.ZhuBajie:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Zhubajie)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.ShaWujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Shawujing)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.BaiLongma:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Bailongma)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.Monkid:
                            break;
                        default:
                            return GameObj.invalidID;
                    }
                }
                else
                {
                    switch (characterType)
                    {
                        case characterType.Null:
                            return GameObj.invalidID;
                        case characterType.JiuLing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Jiuling)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.HongHaier:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Honghaier)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.NiuMowang:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Niumowang)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.TieShan:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Tieshan)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.ZhiZhujing:
                            if (teamList[(int)playerInitInfo.teamID].CharacterPool.GetNum(CharacterType.Zhizhujing)
                                >= GameData.MaxCharacterNum)
                            {
                                return GameObj.invalidID;
                            }
                            break;
                        case characterType.Pawn:
                            break;
                        default:
                            return GameObj.invalidID;
                    }
                }
                characterType? newCharacter = CharacterManager.AddCharacter(playerInitInfo.teamID,
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
    }
}