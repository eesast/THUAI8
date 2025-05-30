using GameClass.GameObj;
using GameClass.GameObj.Areas;
using GameClass.GameObj.Equipments;
using GameClass.GameObj.Map;
using Microsoft.Extensions.Logging;
using Preparation.Utility;
using Preparation.Utility.Value;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Timothy.FrameRateTask;


namespace Gaming
{
    public partial class Game
    {
        private readonly CharacterManager characterManager;
        private class CharacterManager(Game game, Map gameMap)
        {
            private readonly Game game = game;
            private readonly Map map = gameMap;
            public static Character? AddCharacter(long teamID, long playerID, CharacterType charactertype, MoneyPool moneypool)
            {
                Character newcharacter = new(GameData.CharacterRadius, charactertype, moneypool);
                newcharacter.TeamID.SetROri(teamID);
                newcharacter.PlayerID.SetROri(playerID);
                return newcharacter;
            }

            public bool ActivateCharacter(Character character, XY pos)
            {
                if (character.CharacterState2 != CharacterState.DECEASED)
                {
                    return false;
                }
                gameMap.Add(character);
                character.ReSetPos(pos);
                character.SetCharacterState(CharacterState.NULL_CHARACTER_STATE, CharacterState.IDLE);
                new Thread
                (
                    () =>
                    {
                        Thread.Sleep(GameData.CheckInterval);
                        new FrameRateTaskExecutor<int>
                        (
                            loopCondition: () => gameMap.Timer.IsGaming,
                            loopToDo: () =>
                            {
                                CheckSkillTime(character);
                                CheckBerkserk(character);
                                CheckBlind(character);
                                CheckBurned(character);
                                CheckCage(character);
                                CheckCrazyManTime(character);
                                CheckHarmCut(character);
                                CheckHole(character);
                                CheckInvisibility(character);
                                CheckPurified(character);
                                CheckQuickStepTime(character);
                                CheckShoes(character);
                                CheckStunned(character);
                                CheckWideViewTime(character);
                            },
                            timeInterval: GameData.CheckInterval,
                            finallyReturn: () => 0
                        ).Start();
                    }
                ).Start();
                return true;
            }

            public void BeAttacked(Character character, Character obj)
            {
                if (obj.TeamID.Get() == character.TeamID.Get())
                {
                    return;
                }
                long subHP = (long)(obj.AttackPower * (1 - character.HarmCut));
                var team0 = game.TeamList[(int)obj.TeamID.Get()];
                team0.MoneyPool.AddScore(subHP * 20);
                /*if (character.Shield > 0)
                {
                    character.Shield.SubPositiveV(subHP);
                }
                else
                {
                    character.HP.SubPositiveV(subHP);
                }*/
                character.NiuShield.SubPositiveV(subHP);
                if (character.NiuShield > 0)
                {
                    return;
                }
                subHP -= character.NiuShield;
                character.Shield.SubPositiveV(subHP);
                if (character.Shield > 0)
                {
                    return;
                }
                subHP -= character.Shield;
                character.IsShield = false;
                character.HP.SubPositiveV(subHP);
                if (character.HP == 0)
                {
                    long score = 0;
                    if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                    {
                        score = 200000;
                        gameMap.Timer.EndGame();
                    }
                    else if (character.CharacterType == CharacterType.Monkid || character.CharacterType == CharacterType.Pawn)
                        score = 500;
                    else
                        score = character.GetCost();
                    //此处缺失加分代码。由于阵营是分明的（妖怪和取经团队，THUAI7阵营并无明显差别），可以直接将得分加至相应阵营。小局结束后再加到队伍得分。
                    var team = game.TeamList[(int)obj.TeamID.Get()];
                    team.MoneyPool.AddScore(score);
                    Remove(character);
                }
            }
            public void BeAttacked(Character character, long AP)//此部分适用于中立资源攻击及技能攻击
            {
                long subHP = (long)(AP * (1 - character.HarmCut));
                /*if (character.Shield > 0)
                {
                    character.Shield.SubPositiveV(subHP);
                }
                else
                {
                    character.HP.SubPositiveV(subHP);
                }*/
                character.NiuShield.SubPositiveV(subHP);
                if (character.NiuShield > 0)
                {
                    return;
                }
                subHP -= character.NiuShield;
                character.Shield.SubPositiveV(subHP);
                if (character.Shield > 0)
                {
                    return;
                }
                subHP -= character.Shield;
                character.IsShield = false;
                character.HP.SubPositiveV(subHP);
                if (character.HP == 0)
                {
                    long score = 0;
                    if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                    {
                        score = 200000;
                        gameMap.Timer.EndGame();
                    }
                    else if (character.CharacterType == CharacterType.Monkid || character.CharacterType == CharacterType.Pawn)
                        score = 1000;
                    else
                        score = character.GetCost();
                    var team = game.TeamList[1 - (int)character.TeamID.Get()];
                    team.MoneyPool.AddScore(score);
                    Remove(character);
                }
            }
            public void BeAttacked(Character character, A_Resource obj)
            {
                long subHP = (long)(obj.AttackPower * (1 - character.HarmCut));
                /*if (character.Shield > 0)
                {
                    character.Shield.SubPositiveV(subHP);
                }
                else
                {
                    character.HP.SubPositiveV(subHP);
                }*/
                character.NiuShield.SubPositiveV(subHP);
                if (character.NiuShield > 0)
                {
                    return;
                }
                subHP -= character.NiuShield;
                character.Shield.SubPositiveV(subHP);
                if (character.Shield > 0)
                {
                    return;
                }
                subHP -= character.Shield;
                character.IsShield = false;
                character.HP.SubPositiveV(subHP);
                if (character.HP == 0)
                {
                    long score = 0;
                    if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                    {
                        score = 200000;
                        gameMap.Timer.EndGame();
                    }
                    else if (character.CharacterType == CharacterType.Monkid || character.CharacterType == CharacterType.Pawn)
                        score = 500;
                    else
                        score = character.GetCost();
                    //此处缺失加分代码。由于阵营是分明的（妖怪和取经团队，THUAI7阵营并无明显差别），可以直接将得分加至相应阵营。小局结束后再加到队伍得分。
                    Remove(character);
                }
            }
            public bool Recover(Character character, long recover)
            {
                if (recover <= 0)
                    return false;
                character.HP.AddPositiveV(recover);
                return true;
            }

            public void Remove(Character character)
            {
                if (!character.TryToRemoveFromGame(CharacterState.DECEASED))
                {
                    return;
                }
                gameMap.Remove(character);
            }
            public bool Recycle(Character character)
            {
                long characterValue =
                    (long)(character.GetCost() * character.HP.GetDivideValueByMaxV() * GameData.RecycleRate);
                LogicLogging.logger.LogDebug(
                    LoggingFunctional.CharacterLogInfo(character)
                    + $" 's value is {characterValue}");
                character.AddMoney(characterValue);
                LogicLogging.logger.LogDebug(
                    LoggingFunctional.CharacterLogInfo(character)
                    + " is recycled!");
                Remove(character);
                return false;
            }
            public bool ImproveATK(Character character, long ATK)
            {
                if (ATK <= 0)
                    return false;
                character.AttackPower.SetMaxV(character.AttackPower + ATK);
                character.AttackPower.AddPositiveV(ATK);//暂未添加时间限制
                return true;
            }
            public bool ImproveSpeed(Character character, long speed)
            {
                if (speed <= 0)
                    return false;
                character.Shoes.AddPositiveV(speed);//暂未添加时间限制
                return true;
            }
            public void InHole(HOLE trap, Character character)
            {

                if (!character.trapped && character.InSquare(trap.Position, GameData.TrapRange) && trap.TeamID != character.TeamID)
                {
                    character.visible = true;
                    character.trapped = true;
                    character.TrapTime = Environment.TickCount64;
                    character.HP.SubPositiveV(GameData.TrapDamage);
                    //character.SetCharacterState(CharacterState.STUNNED);
                }
            }
            public void CheckHole(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (character.trapped)
                {
                    if (nowtime - character.TrapTime >= 5000)
                    {
                        character.TrapTime = 0;
                        character.trapped = false;
                    }
                    else
                    {
                        if (character.trapped)
                        {
                            if ((nowtime - character.TrapTime) % 1000 <= 5 || (nowtime - character.TrapTime) % 1000 >= 995)
                            {
                                BeAttacked(character, GameData.TrapDamage);
                            }
                        }
                    }
                }
            }
            public void InCage(Cage cage, Character character)
            {
                if (!character.caged && character.InSquare(cage.Position, GameData.TrapRange) && cage.TeamID != character.TeamID)
                {
                    character.visible = true;
                    character.caged = true;
                    character.stunned = true;
                    character.CageTime = Environment.TickCount64;
                    //HP.SubV(GameData.TrapDamage);
                    character.SetCharacterState(CharacterState.STUNNED);
                }
            }
            public void CheckCage(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.CageTime >= 30000 && character.caged)
                {
                    character.CageTime = 0;
                    character.caged = false;
                }
            }
            public void CheckBurned(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (character.burned)
                {
                    if (nowtime - character.BurnedTime >= 5000)
                    {
                        character.BurnedTime = 0;
                        character.burned = false;
                    }
                    else
                    {
                        if (character.burned)
                        {
                            if ((nowtime - character.BurnedTime) % 1000 <= 5 || (nowtime - character.BurnedTime) % 1000 >= 995)
                            {
                                BeAttacked(character, GameData.HongHaierSkillATK);
                            }
                        }
                    }
                }
            }
            public void CheckBlind(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.BlindTime >= 5000 && character.blind)
                {
                    character.BlindTime = 0;
                    character.blind = false;
                }
            }
            public void CheckStunned(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.StunnedTime >= 5000 && character.stunned)
                {
                    character.StunnedTime = 0;
                    character.stunned = false;
                }
            }
            public void CheckHarmCut(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.HarmCutTime >= 15000 && character.HarmCut > 0)
                {
                    character.HarmCutTime = 0;
                    character.HarmCut = 0;
                }
            }
            public void CheckSkillTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.GetSkillTime() >= 60000)
                {
                    character.canskill = true;
                    character.ResetSkillCD();
                }
            }
            public void CheckCrazyManTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.CrazyManTime >= (15 + character.CrazyManNum * 15) * 1000 && character.CrazyManNum != 0)
                {
                    character.AttackPower.SubPositiveV(5 + character.CrazyManNum * 5);
                    character.CrazyManTime = 0;
                    character.CrazyManNum = 0;
                }
            }
            public void CheckQuickStepTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.QuickStepTime >= 60000 && character.QuickStep)
                {
                    character.Shoes.SubPositiveV(500);
                    character.QuickStepTime = 0;
                }
            }
            public void CheckWideViewTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.WideViewTime >= 60000 && character.CanSeeAll)
                {
                    character.CanSeeAll = false;
                    character.WideViewTime = 0;
                }
            }
            public void CheckPurified(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.PurifiedTime >= 30000 && character.Purified)
                {
                    character.Purified = false;
                    character.PurifiedTime = 0;

                }
            }
            public void CheckBerkserk(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (character.IsBerserk)
                {
                    if (nowtime - character.BerserkTime >= GameData.CrazyTime)
                    {
                        character.AttackPower.SetRNow(character.Occupation.AttackPower);
                        character.Shoes.SubPositiveV(GameData.CrazySpeed);
                        character.ATKFrequency = GameData.ATKFreq;
                        character.BerserkTime = 0;
                        character.IsBerserk = false;
                    }
                }
            }
            public void CheckShoes(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (character.IsShoes)
                {
                    if (nowtime - character.ShoesTime >= GameData.ShoesTime)
                    {
                        character.Shoes.SubPositiveV(GameData.ShoesSpeed);
                        character.ShoesTime = 0;
                        character.IsShoes = false;
                    }
                }
            }
            public void CheckInvisibility(Character character)
            {
                //int nowtime = gameMap.Timer.NowTime();
                long nowtime = Environment.TickCount64;
                if (!character.visible)
                {
                    if (nowtime - character.InvisibleTime >= GameData.InvisibleTime)
                    {
                        character.visible = true;
                        character.InvisibleTime = 0;
                    }
                }
                if (gameMap.Timer.NowTime() >= GameData.SevenMinutes)
                {
                    character.visible = true;
                }
            }
        }
    }
}