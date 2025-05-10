using GameClass.GameObj;
using GameClass.GameObj.Map;
using GameClass.GameObj.Areas;
using GameEngine;
using Preparation.Utility;
using System;
using System.Threading;
using Timothy.FrameRateTask;
using Preparation.Utility.Value;

namespace Gaming
{
    public partial class Game
    {
        private readonly ActionManager actionManager;
        private class ActionManager(Game game, Map gameMap, CharacterManager characterManager)
        {
            private readonly Game game = game;
            private readonly Map gameMap = gameMap;
            private readonly CharacterManager characterManager = characterManager;
            private readonly Random random = new();
            public readonly MoveEngine moveEngine = new(
                    gameMap: gameMap,
                    OnCollision: (obj, collisionObj, moveVec) =>
                    {
                        Character ship = (Character)obj;
                        return MoveEngine.AfterCollision.MoveMax;
                    },
                    EndMove: obj =>
                    {
                        obj.ThreadNum.Release();
                    }
                );
            public bool MoveCharacter(Character characterToMove, int moveTimeInMilliseconds, double moveDirection)
            {
                if (moveTimeInMilliseconds < 5)
                {
                    ActionManagerLogging.logger.ConsoleLogDebug("Move time is too short");
                    return false;
                }
                long stateNum = characterToMove.SetCharacterState(CharacterState.MOVING, characterToMove.CharacterState2);
                if (stateNum == -1)
                {
                    ActionManagerLogging.logger.ConsoleLogDebug("Character is not commandable");
                    return false;
                }
                new Thread
                (
                    () =>
                    {
                        characterToMove.ThreadNum.WaitOne();
                        if (!characterToMove.StartThread(stateNum))
                        {
                            characterToMove.ThreadNum.Release();
                            return;
                        }
                        moveEngine.MoveObj(characterToMove, moveTimeInMilliseconds, moveDirection, characterToMove.StateNum, characterToMove.Shoes);
                        Thread.Sleep(moveTimeInMilliseconds);
                        characterToMove.ResetCharacterState(stateNum);
                    }
                )
                { IsBackground = true }.Start();
                return true;
            }
            public bool KnockBackCharacter(Character characterToMove, double moveDirection)
            {
                long stateNum = characterToMove.SetCharacterState(characterToMove.CharacterState1, CharacterState.KNOCKED_BACK);
                CharacterState tempState = characterToMove.CharacterState2;
                if (stateNum == -1)
                {
                    ActionManagerLogging.logger.ConsoleLogDebug("Character can not be knocked back");
                    return false;
                }
                new Thread
                (
                    () =>
                    {
                        characterToMove.ThreadNum.WaitOne();
                        if (!characterToMove.StartThread(stateNum))
                        {
                            characterToMove.ThreadNum.Release();
                            return;
                        }
                        moveEngine.MoveObj(characterToMove, GameData.KnockedBackTime, moveDirection, characterToMove.StateNum, GameData.KnockedBackSpeed);
                        Thread.Sleep(GameData.KnockedBackTime);
                        characterToMove.SetCharacterState(characterToMove.CharacterState1, tempState);
                        characterToMove.ResetCharacterState(stateNum);
                    }
                )
                { IsBackground = true }.Start();
                return true;
            }
            public static bool Stop(Character character)
            {
                lock (character.ActionLock)
                {
                    if (character.Commandable())
                    {
                        character.SetCharacterState(CharacterState.IDLE, character.CharacterState2);
                        return true;
                    }
                }
                return false;
            }
            public bool Produce(Character character)
            {
                E_Resource? Eresource = (E_Resource?)gameMap.OneForInteract(character.Position, GameObjType.ECONOMY_RESOURCE);
                if (Eresource == null)
                {
                    return false;
                }
                if (Eresource.HP == 0)
                {
                    return false;
                }
                long stateNum = character.SetCharacterState(CharacterState.HARVESTING, character.CharacterState2);
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
                        new FrameRateTaskExecutor<int>
                        (
                            loopCondition: () => stateNum == character.StateNum && gameMap.Timer.IsGaming,
                            loopToDo: () =>
                            {
                                if (!Eresource.Produce(GameData.ProduceSpeedPerSecond / GameData.FrameDuration, character))
                                {
                                    //character.ResetShipState(stateNum);
                                    return false;
                                }
                                if (Eresource.HP == 0)
                                {
                                    //character.ResetShipState(stateNum);
                                    return false;
                                }
                                return true;
                            },
                            timeInterval: GameData.CheckInterval,
                            finallyReturn: () => 0
                        ).Start();
                        character.ThreadNum.Release();
                        //Eresource.SubProduceNum();
                    }
                )
                { IsBackground = true }.Start();
                return false;
            }
            public bool SetTrap(Character character, TrapType traptype)
            {
                if (character.CharacterType != CharacterType.Monkid && character.CharacterType != CharacterType.Pawn)
                {
                    return false;
                }
                long stateNum = character.SetCharacterState(CharacterState.CONSTRUCTING, character.CharacterState2);
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
                        Thread.Sleep(GameData.CheckInterval);
                        new FrameRateTaskExecutor<int>
                        (
                            loopCondition: () => stateNum == character.StateNum && gameMap.Timer.IsGaming,
                            loopToDo: () =>
                            {
                                CellXY nowPos = GameData.PosGridToCellXY(character.Position);
                                character.ResetCharacterState(stateNum);
                                switch (traptype)
                                {
                                    case TrapType.CAGE:
                                        gameMap.Add(new Cage(GameData.GetCellCenterPos(nowPos.x, nowPos.y)));
                                        Cage? cage = (Cage?)gameMap.OneForInteract(character.Position, GameObjType.TRAP);
                                        cage.SetCage(character);
                                        cage.IsActivated.Set(true);
                                        new Thread
                                                (
                                                    () =>
                                                    {
                                                        Thread.Sleep(GameData.CheckInterval);
                                                        new FrameRateTaskExecutor<int>
                                                        (
                                                            loopCondition: () =>
                                                                gameMap.Timer.IsGaming && cage != null,
                                                            loopToDo: () =>
                                                            {
                                                                var characters = gameMap.CharacterInTheRangeNotTeamID(
                                                                    cage.Position, GameData.TrapRange, cage.TeamID);
                                                                if (characters == null || characters.Count == 0)
                                                                {
                                                                    return true;
                                                                }
                                                                foreach (var character in characters)
                                                                {
                                                                    characterManager.InCage(cage, character);
                                                                }
                                                                gameMap.Remove(cage);//实时捕捉，用后即毁
                                                                return true;
                                                            },
                                                            timeInterval: GameData.CheckInterval,
                                                            finallyReturn: () => 0
                                                        ).Start();
                                                    }
                                                )
                                        { IsBackground = true }.Start();
                                        break;
                                    case TrapType.HOLE:
                                        gameMap.Add(new HOLE(GameData.GetCellCenterPos(nowPos.x, nowPos.y)));
                                        HOLE? hole = (HOLE?)gameMap.OneForInteract(character.Position, GameObjType.TRAP);
                                        hole.SetHole(character);
                                        hole.IsActivated.Set(true);
                                        new Thread
                                                (
                                                    () =>
                                                    {
                                                        Thread.Sleep(GameData.CheckInterval);
                                                        new FrameRateTaskExecutor<int>
                                                        (
                                                            loopCondition: () =>
                                                                gameMap.Timer.IsGaming && hole != null,
                                                            loopToDo: () =>
                                                            {
                                                                var characters = gameMap.CharacterInTheRangeNotTeamID(
                                                                    hole.Position, GameData.TrapRange, hole.TeamID);
                                                                if (characters == null || characters.Count == 0)
                                                                {
                                                                    return true;
                                                                }
                                                                foreach (var character in characters)
                                                                {
                                                                    characterManager.InHole(hole, character);
                                                                }
                                                                gameMap.Remove(hole);//实时捕捉，用后即毁
                                                                return true;
                                                            },
                                                            timeInterval: GameData.CheckInterval,
                                                            finallyReturn: () => 0
                                                        ).Start();
                                                    }
                                                )
                                        { IsBackground = true }.Start();
                                        break;
                                }
                                return true;
                            },
                            timeInterval: GameData.CheckInterval,
                            finallyReturn: () => 0
                        ).Start();
                        character.ThreadNum.Release();
                    }
                )
                { IsBackground = true }.Start();
                return false;
            }
            public bool Construct(Character character, ConstructionType constructionType)
            {
                Construction? construction = (Construction?)gameMap.OneForInteract(character.Position, GameObjType.CONSTRUCTION);
                if (character.CharacterType != CharacterType.Monkid || character.CharacterType != CharacterType.Pawn)
                {
                    return false;
                }
                if (construction == null)
                {
                    return false;
                }
                if (construction.HP.IsMaxV())
                {
                    return false;
                }
                if (constructionType == ConstructionType.HOLE || constructionType == ConstructionType.CAGE)
                {
                    return false;
                }
                long stateNum = character.SetCharacterState(CharacterState.CONSTRUCTING, character.CharacterState2);
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
                        construction.AddConstructNum();
                        Thread.Sleep(GameData.CheckInterval);
                        new FrameRateTaskExecutor<int>
                        (
                            loopCondition: () => stateNum == character.StateNum && gameMap.Timer.IsGaming,
                            loopToDo: () =>
                            {
                                if (!construction.Construct(constructionType, character))
                                {
                                    character.ResetCharacterState(stateNum);
                                    return false;
                                }
                                if (construction.HP.IsMaxV())
                                {
                                    //ship.ResetShipState(stateNum);
                                    if (!construction.IsActivated)
                                    {
                                        switch (construction.ConstructionType)
                                        {
                                            case ConstructionType.FARM:
                                                game.AddFactory(construction.TeamID);
                                                break;
                                            case ConstructionType.BARRACKS:
                                                game.AddBirthPoint(construction.TeamID, construction.Position);
                                                break;
                                        }
                                        construction.IsActivated.Set(true);
                                    }
                                    return false;
                                }
                                return true;
                            },
                            timeInterval: GameData.CheckInterval * 100,
                            finallyReturn: () => 0
                        ).Start();
                        character.ThreadNum.Release();
                        construction.SubConstructNum();
                    }
                )
                { IsBackground = true }.Start();
                return false;
            }
            public bool TeamTask(Base team)
            {
                new Thread
                (
                    () =>
                    {
                        while (!gameMap.Timer.IsGaming)
                        {
                            Thread.Sleep(1);
                        }
                        new FrameRateTaskExecutor<int>
                        (
                            loopCondition: () => gameMap.Timer.IsGaming,
                            loopToDo: () =>
                            {
                                team.AddMoney(team.MoneyAddPerSecond / GameData.NumOfStepPerSecond);
                                return true;
                            },
                            timeInterval: GameData.CheckInterval,
                            finallyReturn: () => 0
                        ).Start();
                    }
                )
                { IsBackground = true }.Start();
                return false;
            }
        }
    }
}
