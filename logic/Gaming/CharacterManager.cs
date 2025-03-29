using GameClass.GameObj;
using GameClass.GameObj.Areas;
using GameClass.GameObj.Equipments;
using GameClass.GameObj.Map;
using Preparation.Utility;
using Preparation.Utility.Value;
using System.Threading;
using System.Threading.Tasks.Dataflow;


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
                character.SetCharacterState(CharacterState.NULL_CHARACTER_STATE, CharacterState.NULL_CHARACTER_STATE);
                return true;
            }

            public void BeAttacked(Character character, Character obj)
            {
                if (obj.TeamID.Get() == character.TeamID.Get())
                {
                    return;
                }
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
                if(character.NiuShield > subHP)
                {
                    break;
                }
                subHP -= character.NiuShield;
                character.Shield.SubPositiveV(subHP);
                else if(character.Shiled > subHP)
                {
                    break;
                }
                subHP -= character.Shield;
                else
                {
                    character.HP.SubPositiveV(subHP);
                }         
                if (character.HP == 0)
                {
                    long score = 0;
                    if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                        score = 200000;
                    else if (character.CharacterType == CharacterType.Monkid || character.CharacterType == CharacterType.Pawn)
                        score = 500;
                    else
                        score = character.GetCost();
                    //此处缺失加分代码。由于阵营是分明的（妖怪和取经团队，THUAI7阵营并无明显差别），可以直接将得分加至相应阵营。小局结束后再加到队伍得分。
                    var team = game.TeamList[(int)character.TeamID.Get()];
                    team.MoneyPool.SubScore(score);
                    Remove(character);
                }
            }
            public void BeAttacked(Character character, long AP)//此部分适用于中立资源攻击及技能攻击
            {
                long subHP = (long)(AP * (1 - character.HarmCut));
                if (character.Shield > 0)
                {
                    character.Shield.SubPositiveV(subHP);
                }
                else
                {
                    character.HP.SubPositiveV(subHP);
                }
                if (character.HP == 0)
                {
                    long score = 0;
                    if (character.CharacterType == CharacterType.TangSeng || character.CharacterType == CharacterType.JiuLing)
                        score = 200000;
                    else if (character.CharacterType == CharacterType.Monkid || character.CharacterType == CharacterType.Pawn)
                        score = 500;
                    else
                        score = character.GetCost();
                    var team = game.TeamList[(int)character.TeamID.Get()];
                    team.MoneyPool.SubScore(score);
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
                CharacterManagerLogging.logger.ConsoleLogDebug(
                    LoggingFunctional.CharacterLogInfo(character)
                    + $" 's value is {characterValue}");
                character.AddMoney(characterValue);
                CharacterManagerLogging.logger.ConsoleLogDebug(
                    LoggingFunctional.CharacterLogInfo(character)
                    + " is recycled!");
                Remove(character);
                return false;
            }
            public bool ImproveATK(Character character, long ATK)
            {
                if (ATK <= 0)
                    return false;
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
            public void InTrap(Trap trap, Character character)
            {
                if (!character.trapped && character.InSquare(trap.Position, GameData.TrapRange) && trap.TeamID != character.TeamID)
                {
                    character.visible = true;
                    character.trapped = true;
                    character.TrapTime = Environment.TickCount64;
                    //HP.SubV(GameData.TrapDamage);
                    //SetCharacterState(CharacterState.STUNNED);
                }
            }
            public void CheckTrap(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.TrapTime >= 5000)
                {
                    character.TrapTime = long.MaxValue;
                    character.trapped = false;
                }
                else
                {
                    if ((nowtime - character.TrapTime) % 1000 <= 25 || (nowtime - character.TrapTime) % 1000 >= 975)
                    {
                        BeAttacked(character, GameData.TrapDamage);
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
                    //SetCharacterState(CharacterState.STUNNED);

                }
            }
            public void CheckCage(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.CageTime >= 30000)
                {
                    character.CageTime = long.MaxValue;
                    character.caged = false;
                }
            }
            public void CheckBurned(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.BurnedTime >= 5000)
                {
                    character.BurnedTime = long.MaxValue;
                    character.burned = false;
                }
                else
                {
                    if ((nowtime - character.TrapTime) % 1000 <= 25 || (nowtime - character.TrapTime) % 1000 >= 975)
                    {
                        BeAttacked(character, GameData.HongHaierSkillATK);
                    }
                }
            }
            public void CheckBlind(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.BlindTime >= 5000)
                {
                    character.BlindTime = long.MaxValue;
                    character.blind = false;
                }
            }
            public void CheckStunned(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.StunnedTime >= 5000)
                {
                    character.StunnedTime = long.MaxValue;
                    character.stunned = false;
                }
            }
            public void CheckHarmCut(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.BurnedTime >= 15000)
                {
                    character.HarmCutTime = long.MaxValue;
                    character.HarmCut = 0;
                }
            }
            public void CheckSkillTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.GetSkillTime() >= 6000)
                {
                    character.canskill = true;
                    character.ResetSkillCD();
                }
            }
            public void CheckCrazyManTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.CrazyManTime >= (15 + character.CrazyManNum * 15))
                {
                    character.AttackPower.SubPositiveV(5 + character.CrazyManNum * 5);
                }
            }
            public void CheckQuickStepTime(Character character)
            {
                long nowtime = Environment.TickCount64;
                if (nowtime - character.CrazyManTime >= 60000)
                {
                    character.Shoes.SubPositiveV(500);
                }
            }
        }
    }
}
