using GameClass.GameObj;
using GameClass.GameObj.Map;
using Preparation.Utility;
using Preparation.Utility.Value;
using System.Threading;


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
                long subHP = obj.AttackPower;
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
                    //此处缺失加分代码。由于阵营是分明的（妖怪和取经团队，THUAI7阵营并无明显差别），可以直接将得分加至相应阵营。小局结束后再加到队伍得分。
                    Remove(character);
                }
            }

            public void BeAttacked(Character character, long AP)//此部分适用于中立资源攻击及技能攻击
            {
                if (character.Shield > 0)
                {
                    character.Shield.SubPositiveV(AP);
                }
                else
                {
                    character.HP.SubPositiveV(AP);
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
                    Remove(character);
                }
            }

            public static bool Recover(Character character, long recover)
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
            public static bool ImproveATK(Character character, long ATK)
            {
                if (ATK <= 0)
                    return false;
                character.AttackPower.AddPositiveV(ATK);//暂未添加时间限制
                return true;
            }
            public static bool ImproveSpeed(Character character, long speed)
            {
                if (speed <= 0)
                    return false;
                character.Shoes.AddPositiveV(speed);//暂未添加时间限制
                return true;
            }
        }
    }
}
