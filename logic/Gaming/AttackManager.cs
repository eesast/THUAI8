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
        private readonly AttackManager attackManager;
        private class AttackManager
        {
            private readonly Game game;
            private readonly Map gameMap;
            private readonly CharacterManager characterManager;
            private readonly MoveEngine moveEngine;
            private readonly A_ResourceManager ARManager;
            public AttackManager(Game game, Map gameMap, CharacterManager characterManager)
            {
                this.game = game;
                this.gameMap = gameMap;
                this.characterManager = characterManager;
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
            public bool Attack(Character character, Character gameobj)
            {
                if (!character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                if (!gameMap.InAttackSize(character, gameobj))
                {
                    return false;
                }
                if (gameobj.visible == false || gameobj.CharacterState2 == CharacterState.INVISIBLE)
                {
                    return false;
                }
                long nowtime = Environment.TickCount64;
                if (nowtime - character.LastAttackTime < 1 / (character.ATKFrequency * 1000))
                    return false;
                characterManager.BeAttacked(gameobj, character);
                character.LastAttackTime = nowtime;
                if (character.CharacterState2 == CharacterState.INVISIBLE || character.visible == false)
                {
                    character.visible = true;
                    character.SetCharacterState(character.CharacterState1, CharacterState.NULL_CHARACTER_STATE);//破隐
                }
                return true;
            }
            public bool Attack(Character character, A_Resource gameobj)
            {
                if (!character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                if (!gameMap.InAttackSize(character, gameobj))
                {
                    return false;
                }
                long nowtime = Environment.TickCount64;
                if (nowtime - character.LastAttackTime < 1 / (character.ATKFrequency * 1000))
                    return false;
                ARManager.BeAttacked(gameobj, character);
                character.LastAttackTime = nowtime;
                if (character.CharacterState2 == CharacterState.INVISIBLE)
                    character.SetCharacterState(character.CharacterState1, CharacterState.NULL_CHARACTER_STATE);//破隐
                return true;
            }
            public bool Attack(Character character, Construction gameobj)
            {
                if (!character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                if (!gameMap.InAttackSize(character, gameobj))
                {
                    return false;
                }
                long nowtime = Environment.TickCount64;
                if (nowtime - character.LastAttackTime < 1 / (character.ATKFrequency * 1000))
                    return false;
                gameobj.BeAttacked(character);
                character.LastAttackTime = nowtime;
                if (character.CharacterState2 == CharacterState.INVISIBLE)
                    character.SetCharacterState(character.CharacterState1, CharacterState.NULL_CHARACTER_STATE);//破隐
                return true;
            }
            public bool AttackResource(Character character)
            {
                A_Resource? Aresource = (A_Resource?)gameMap.OneForInteract(character.Position, GameObjType.ADDITIONAL_RESOURCE);
                if (Aresource == null)
                {
                    return false;
                }
                if (Aresource.HP == 0)
                {
                    return false;
                }
                long stateNum = character.SetCharacterState(CharacterState.ATTACKING, character.CharacterState2);
                if (stateNum == -1)
                {
                    return false;
                }
                new Thread
                (
                    () =>
                    {
                        character.ThreadNum.WaitOne();
                        if (!character.StartThread(stateNum))
                        {
                            character.ThreadNum.Release();
                            return;
                        }
                        //Eresource.AddProduceNum();
                        Thread.Sleep(GameData.CheckInterval);
                        Attack(character, Aresource);
                    }
                )
                { IsBackground = true }.Start();
                return false;
            }
        }
    }
}
