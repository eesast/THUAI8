using GameClass.GameObj;
using GameClass.GameObj.Map;
using Preparation.Utility;
using Preparation.Utility.Value;
using GameClass.GameObj.Areas;


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
                                score = 4000;
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
                                score = 5000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveATK(teamcharacter, 15))
                                    {
                                        teamcharacter.CrazyManTime = Environment.TickCount64;
                                        teamcharacter.CrazyManNum = 1;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.CRAZY_MAN3:
                            {
                                score = 6000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveATK(teamcharacter, 20))
                                    {
                                        teamcharacter.CrazyManTime = Environment.TickCount64;
                                        teamcharacter.CrazyManNum = 1;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL1:
                            {
                                score = 2000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 50);
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL2:
                            {
                                score = 3000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 100);
                                }
                            }
                            break;
                        case A_ResourceType.LIFE_POOL3:
                            {
                                score = 4000;
                                foreach (var teamcharacter in characters)
                                {
                                    characterManager.Recover(teamcharacter, 150);
                                }
                            }
                            break;
                        case A_ResourceType.QUICK_STEP:
                            {
                                score = 3000;
                                foreach (var teamcharacter in characters)
                                {
                                    if (characterManager.ImproveSpeed(teamcharacter, 500))
                                    {
                                        teamcharacter.QuickStepTime = Environment.TickCount64;
                                    }
                                }
                            }
                            break;
                        case A_ResourceType.WIDE_VIEW:
                            { score = 3000; }
                            break;
                    }//此部分缺失加得分代码
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
        }
    }
}
