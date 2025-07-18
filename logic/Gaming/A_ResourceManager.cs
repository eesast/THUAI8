﻿using GameClass.GameObj;
using GameClass.GameObj.Areas;
using GameClass.GameObj.Equipments;
using GameClass.GameObj.Map;
using Preparation.Utility;
using Preparation.Utility.Value;


namespace Gaming
{
    public partial class Game
    {
        private readonly A_ResourceManager ARManager;
        private class A_ResourceManager(Game game, Map gameMap, CharacterManager characterManager)
        {
            private readonly Game game = game;
            private readonly Map map = gameMap;
            private readonly CharacterManager characterManager = characterManager;
            private readonly Random random = new();
            public static A_Resource? AddAResource(A_ResourceType type, XY pos)
            {
                A_Resource NewAResource = new(GameData.AResourceRadius, type, pos);
                return NewAResource;
            }
            public bool activateAR(A_Resource AResource)
            {
                if (AResource.ARstate == AdditionResourceState.BEATEN)
                {
                    return false;
                }
                gameMap.Add(AResource);
                AResource.SetARState(AdditionResourceState.BEATABLE);
                return true;
            }
            public bool BeAttacked(A_Resource AResource, Character character)
            {
                if (AResource.ARstate == AdditionResourceState.BEATEN)
                    return false;
                long subHP = character.AttackPower;
                AResource.HP.SubPositiveV(subHP);
                if (AResource.HP == 0)
                {
                    var characters = gameMap.CharacterInTeamID(character.TeamID);
                    long score = 0;
                    switch (AResource.AResourceType)
                    {
                        case A_ResourceType.CRAZY_MAN1:
                            {
                                score = 8000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveATK(teamcharacter, 10))
                                    {
                                        teamcharacter.CrazyManTime = Environment.TickCount64;
                                        teamcharacter.CrazyManNum = 1;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.CRAZY_MAN2:
                            {
                                score = 10000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveATK(teamcharacter, 15))
                                    {
                                        teamcharacter.CrazyManTime = Environment.TickCount64;
                                        teamcharacter.CrazyManNum = 2;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.CRAZY_MAN3:
                            {
                                score = 12000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveATK(teamcharacter, 20))
                                    {
                                        teamcharacter.CrazyManTime = Environment.TickCount64;
                                        teamcharacter.CrazyManNum = 3;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL1:
                            {
                                score = 4000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 50);
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL2:
                            {
                                score = 6000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 100);
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL3:
                            {
                                score = 8000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 150);
                                }
                            }
                            break;
                        case A_ResourceType.QUICK_STEP:
                            {
                                score = 6000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveSpeed(teamcharacter, 500))
                                    {
                                        teamcharacter.QuickStepTime = Environment.TickCount64;
                                        teamcharacter.QuickStep = true;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.WIDE_VIEW:
                            {
                                score = 6000;
                                foreach (var teamcharacter in characters)
                                {
                                    teamcharacter.CanSeeAll = true;
                                    teamcharacter.WideViewTime = Environment.TickCount64;
                                }
                            }
                            break;
                    }
                    var team = game.TeamList[(int)character.TeamID.Get()];
                    team.MoneyPool.AddScore(score);
                    AResource.SetARState(AdditionResourceState.BEATEN);
                    return true;
                }
                AResource.SetARState(AdditionResourceState.BEING_BEATEN);
                return true;
            }
            public bool BeAttacked(A_Resource AResource, long AP)
            {
                if (AResource.ARstate == AdditionResourceState.BEATEN)
                    return false;
                long subHP = AP;
                AResource.HP.SubPositiveV(subHP);
                if (AResource.HP == 0)
                {
                    long score = 0;
                    switch (AResource.AResourceType)
                    {
                        case A_ResourceType.CRAZY_MAN1:
                            score = 4000;
                            break;
                        case A_ResourceType.CRAZY_MAN2:
                            score = 5000;
                            break;
                        case A_ResourceType.CRAZY_MAN3:
                            score = 6000;
                            break;
                        case A_ResourceType.LIFE_POOL1:
                            score = 2000;
                            break;
                        case A_ResourceType.LIFE_POOL2:
                            score = 3000;
                            break;
                        case A_ResourceType.LIFE_POOL3:
                            score = 4000;
                            break;
                        case A_ResourceType.QUICK_STEP:
                            score = 3000;
                            break;
                        case A_ResourceType.WIDE_VIEW:
                            score = 3000;
                            break;
                    }
                    AResource.SetARState(AdditionResourceState.BEATEN);
                    return true;
                }
                AResource.SetARState(AdditionResourceState.BEING_BEATEN);
                return true;
            }
            public void Remove(A_Resource AResource)
            {
                if (!AResource.TryToRemoveFromGame())
                {
                    return;
                }
                gameMap.Remove(AResource);
            }
            public void LevelUpAR(A_Resource AResource)
            {
                int nowtime = gameMap.Timer.NowTime();
                if (nowtime >= GameData.SevenMinutes && nowtime <= GameData.SevenMinutes + 1000 && !AResource.refresh_at_7_min)
                {
                    if (AResource.AResourceType == A_ResourceType.CRAZY_MAN2)
                    {
                        AResource.HP.SetMaxV(GameData.CrazyMan3HP);
                        AResource.HP.SetVToMaxV();
                        AResource.AttackPower.SetMaxV(GameData.CrazyMan3ATK);
                        AResource.AttackPower.SetVToMaxV();
                        AResource.SetARtype(A_ResourceType.CRAZY_MAN3);
                    }
                    else if (AResource.AResourceType == A_ResourceType.LIFE_POOL2)
                    {
                        AResource.HP.SetMaxV(GameData.LifePool3HP);
                        AResource.HP.SetVToMaxV();
                        AResource.AttackPower.SetMaxV(GameData.LifePoolATK);
                        AResource.AttackPower.SetVToMaxV();
                        AResource.SetARtype(A_ResourceType.LIFE_POOL3);
                    }
                    else
                    {
                        AResource.HP.SetVToMaxV();
                    }
                    AResource.SetARState(AdditionResourceState.BEATABLE);
                    AResource.refresh_at_7_min = true;
                }
                else if (nowtime >= GameData.ThreeMinutes && nowtime <= GameData.ThreeMinutes + 1000 && !AResource.refresh_at_3_min)
                {
                    if (AResource.AResourceType == A_ResourceType.CRAZY_MAN1)
                    {
                        AResource.HP.SetMaxV(GameData.CrazyMan2HP);
                        AResource.HP.SetVToMaxV();
                        AResource.AttackPower.SetMaxV(GameData.CrazyMan2ATK);
                        AResource.AttackPower.SetVToMaxV();
                        AResource.SetARtype(A_ResourceType.CRAZY_MAN2);
                    }
                    else if (AResource.AResourceType == A_ResourceType.LIFE_POOL1)
                    {
                        AResource.HP.SetMaxV(GameData.LifePool2HP);
                        AResource.HP.SetVToMaxV();
                        AResource.AttackPower.SetMaxV(GameData.LifePoolATK);
                        AResource.AttackPower.SetVToMaxV();
                        AResource.SetARtype(A_ResourceType.LIFE_POOL2);
                    }
                    else
                    {
                        AResource.HP.SetVToMaxV();
                    }
                    AResource.SetARState(AdditionResourceState.BEATABLE);
                    AResource.refresh_at_3_min = true;
                }
            }
            public void autoAttack(A_Resource aresource)
            {
                var characters = gameMap.AllCharacterInTheRange(aresource.Position, GameData.AdditionResourceAttackRange);
                long nowtime = Environment.TickCount64;
                if (characters == null || characters.Count == 0)
                    return;
                if (nowtime - aresource.LastAttackTime < 1000)
                    return;
                if (nowtime - aresource.LastBeAttackedTime > 5000)
                    return;
                aresource.LastAttackTime = nowtime;
                var character = characters[random.Next(characters.Count)];
                characterManager.BeAttacked(character, aresource);
                return;
            }
        }
    }
}