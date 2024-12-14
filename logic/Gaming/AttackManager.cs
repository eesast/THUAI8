using GameClass.GameObj;
using GameClass.GameObj.Map;
using GameEngine;
using Preparation.Interface;
using Preparation.Utility;
using GameClass.GameObj.Areas;
using Preparation.Utility.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Gaming
{
    public partial class Game
    {
        private readonly AttackManager AttackeManager;
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
                if (character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                characterManager.BeAttacked(gameobj, character);
                return true;
            }
            public bool Attack(Character character, A_Resource gameobj)
            {
                if (character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                ARManager.BeAttacked(gameobj, character);
                return true;
            }
            public bool Attack(Character character, Construction gameobj)
            {
                if (character.Commandable() || character.CharacterState2 == CharacterState.BLIND)
                {
                    return false;
                }
                if (!gameMap.CanSee(character, gameobj))
                {
                    return false;
                }
                gameobj.BeAttacked(character);
                return true;
            }
        }
    }
}
