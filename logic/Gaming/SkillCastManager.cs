using GameClass.GameObj;
using GameClass.GameObj.Areas;
using GameClass.GameObj.Map;
using GameEngine;
using Microsoft.Extensions.Logging;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            private readonly ActionManager actionManager;
            public SkillCastManager(Game game, Map gameMap, CharacterManager characterManager, A_ResourceManager a_ResourceManager, ActionManager actionManager)
            {
                this.game = game;
                this.gameMap = gameMap;
                this.characterManager = characterManager;
                this.ARManager = a_ResourceManager;
                this.actionManager = actionManager;
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
                if (character.CharacterState2 == CharacterState.BLIND || character.blind)
                {
                    SkillCastingManagerLogging.logger.LogDebug("Character is blind!");
                    return false;
                }
                if (!character.canskill)
                {
                    SkillCastingManagerLogging.logger.LogDebug("Skill casting is still in cd!");
                    return false;
                }
                long stateNum = character.SetCharacterState(CharacterState.SKILL_CASTING, character.CharacterState2);
                if (stateNum == -1)
                {
                    SkillCastingManagerLogging.logger.LogDebug("Character is not commandable!");
                    return false;
                }
                character.StartSkillCD();
                character.canskill = false;
                switch (character.CharacterType)
                {
                    case CharacterType.SunWukong:
                        {
                            var ObjBeingShots = gameMap.CharacterOnTheSameLineNotTeamID(character.Position, theta, character.TeamID);
                            if (ObjBeingShots == null || ObjBeingShots.Count == 0)
                            {
                                return true;
                            }
                            foreach (var ObjBeingShot in ObjBeingShots)
                            {
                                switch (ObjBeingShot.Type)
                                {
                                    case GameObjType.CHARACTER:
                                        {
                                            characterManager.BeAttacked(ObjBeingShot, GameData.SunWukongSkillATK);
                                        }
                                        break;
                                    default: break;
                                }
                            }
                            return true;
                        }
                        break;
                    case CharacterType.ZhuBajie:
                        {
                            characterManager.Recover(character, 150);//回复一半血量
                            character.HarmCut = 0.5;//设置伤害减免。此处尚未增加时间限制
                            character.HarmCutTime = Environment.TickCount64;
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
                                    case GameObjType.CHARACTER:
                                        {
                                            if (ObjBeingShot.Purified == true)
                                                continue;
                                            else
                                            {
                                                ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.BLIND);
                                                ObjBeingShot.blind = true;
                                                ObjBeingShot.BlindTime = Environment.TickCount64;
                                            }
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
                                    case GameObjType.CHARACTER:
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
                                    case GameObjType.CHARACTER:
                                        {
                                            ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.BURNED);
                                            ObjBeingShot.burned = true;
                                            ObjBeingShot.BurnedTime = Environment.TickCount64;
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
                                    ObjBeingProtected.NiuShield.AddPositiveV(GameData.NiuMowangShield);
                                    character.NiuShield.AddPositiveV(GameData.NiuMowangShield);
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
                                    case GameObjType.CHARACTER:
                                        {
                                            if (ObjBeingShot.CharacterState2 == CharacterState.BURNED || ObjBeingShot.burned)
                                            {
                                                characterManager.BeAttacked(ObjBeingShot, GameData.TieShanSkillATK);
                                            }
                                            if (ObjBeingShot.Purified == true)
                                                continue;
                                            else
                                            {
                                                ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.KNOCKED_BACK);
                                                double angleToBeKnockedBack;
                                                double tantheta = (ObjBeingShot.Position.y - character.Position.y) / (ObjBeingShot.Position.x - character.Position.x);
                                                if ((ObjBeingShot
                                                    .Position.x - character.Position.x) > 0)
                                                    angleToBeKnockedBack = Math.Atan(tantheta);
                                                else if ((ObjBeingShot.Position.y - character.Position.y) > 0)
                                                    angleToBeKnockedBack = Math.PI - Math.Atan(tantheta);
                                                else
                                                    angleToBeKnockedBack = -Math.PI - Math.Atan(tantheta);
                                                actionManager.KnockBackCharacter(ObjBeingShot, angleToBeKnockedBack);
                                            }
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
                                    case GameObjType.CHARACTER:
                                        {
                                            characterManager.BeAttacked(ObjBeingShot, GameData.ZhiZhujingSkillATK);
                                            if (ObjBeingShot.Purified == true)
                                                continue;
                                            else
                                            {
                                                ObjBeingShot.SetCharacterState(ObjBeingShot.CharacterState1, CharacterState.STUNNED);//尚未加入时间限制
                                                ObjBeingShot.stunned = true;
                                                ObjBeingShot.StunnedTime = Environment.TickCount64;
                                            }
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
