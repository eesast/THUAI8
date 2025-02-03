using GameClass.GameObj;
using GameClass.GameObj.Map;
using Preparation.Interface;
using Preparation.Utility;
using GameClass.GameObj.Areas;
using Preparation.Utility.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameEngine;

namespace Gaming
{
    public partial class Game
    {
        private readonly SkillCastManager skillCastManager;
        private class SkillCastManager
        {
            private readonly Game game;
            private readonly Map gameMap;
            private readonly CharacterManager characterManager;
            private readonly MoveEngine moveEngine;
            private readonly A_ResourceManager ARManager;
            public SkillCastManager(Game game, Map gameMap, CharacterManager characterManager, A_ResourceManager a_ResourceManager)
            {
                this.game = game;
                this.gameMap = gameMap;
                this.characterManager = characterManager;
                this.ARManager = a_ResourceManager;
                moveEngine = new(
                    gameMap: gameMap,
                    OnCollision: (obj, collisionObj, moveVec) =>
                    {
                        return MoveEngine.AfterCollision.Destroyed;
                    },
                    EndMove: obj =>
                    {
                        obj.CanMove.SetROri(false);
                    }
                );
                this.game = game;
            }
            public bool SkillCasting(Character character, double theta = 0.0)
            {
                if (!character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                    return false;
                switch (character.CharacterType)
                {
                    case CharacterType.SunWukong:
                        { }
                        break;
                    case CharacterType.ZhuBajie:
                        {
                            characterManager.Recover(character, 150);//回复一半血量
                            character.HarmCut = 0.5;//设置伤害减免。此处尚未增加时间限制
                        }
                        break;
                    case CharacterType.ShaWujing:
                        {
                            var ObjBeingShots = gameMap.CharacterInTheRangeNotTeamID(character.Position, GameData.SkillRange1, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.Character:
                                        {
                                            ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.BLIND);
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.BaiLongma:
                        {
                            var ObjBeingShots = gameMap.CharacterInTheRangeNotTeamID(character.Position, GameData.SkillRange2, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.Character:
                                        {
                                            characterManager.BeAttacked(ObjBeingShot, GameData.BaiLongmaSkillATK);
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.HongHaier:
                        {
                            var ObjBeingShots = gameMap.CharacterInTheRangeNotTeamID(character.Position, GameData.SkillRange1, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.Character:
                                        {
                                            ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.BURNED);
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.NiuMowang:
                        {
                            var ObjBeingProtecteds = gameMap.CharacterInTheRangeInTeamID(character.Position, GameData.SkillRange1, character.TeamID);
                            if (ObjBeingProtecteds == null || ObjBeingProtecteds.Count == 0)
                            {
                                return true;
                            }
                            long minHP = 1000;
                            foreach (var ObjBeingProtected in ObjBeingProtecteds)
                            {
                                if (ObjBeingProtected.HP < minHP)
                                {
                                    minHP = ObjBeingProtected.HP;
                                }
                            }
                            foreach (var ObjBeingProtected in ObjBeingProtecteds)
                            {
                                if (ObjBeingProtected.HP == minHP)
                                {
                                    ObjBeingProtected.Shield.AddPositiveV(GameData.NiuMowangShield);
                                    character.Shield.AddPositiveV(GameData.NiuMowangShield);
                                    break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.TieShan:
                        {
                            var ObjBeingShots = gameMap.CharacterInTheRangeNotTeamID(character.Position, GameData.SkillRange1, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.Character:
                                        {
                                            if (ObjBeingShot.CharacterState2 == CharacterState.BURNED)
                                            {
                                                characterManager.BeAttacked(ObjBeingShot, GameData.TieShanSkillATK);
                                            }
                                            ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.KNOCKED_BACK);
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.ZhiZhujing:
                        {
                            var ObjBeingShots = gameMap.CharacterInTheRangeNotTeamID(character.Position, GameData.SkillRange1, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.Character:
                                        {
                                            characterManager.BeAttacked(ObjBeingShot, GameData.ZhiZhujingSkillATK);
                                            ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.STUNNED);//尚未加入时间限制
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                }
                return true;
            }
        }
    }
}
