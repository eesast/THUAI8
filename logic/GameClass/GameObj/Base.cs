using GameClass.GameObj.Areas;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue;
using Preparation.Utility.Value.SafeValue.Atomic;
using System.Collections.Generic;

namespace GameClass.GameObj
{
    public class Base : IPlayer
    {
        public AtomicLong TeamID { get; }
        public AtomicLong PlayerID { get; } = new(0);
        public const long invalidTeamID = long.MaxValue;
        public const long noneTeamID = long.MinValue;
        public ObjPool<Character, CharacterType> CharacterPool { get; }
        private List<XY> birthPointList = [];//根据兵营初始化
        public List<XY> BirthPointList => birthPointList;
        public Home Home { get; set; }
        public MoneyPool MoneyPool { get; } = new();
        public AtomicInt BarrackNum { get; } = new(0);
        public AtomicInt FarmNum { get; } = new(1);
        public int MoneyAddPerSecond => FarmNum * GameData.ScoreFarmPerSecond;
        public Base(Construction construction)
        {
            TeamID = new(construction.TeamID);
            Construction = construction;
            CharacterPool = new(
                classfier: (character) => character.CharacterType,//以下可能出问题
                idleChecker: (character) => character.IsRemoved,
                idleChecker: (character) => character.IsRemoved,
                tryActivator: (character) =>
                {
                    if (character.IsRemoved.TrySet(false))
                    {
                        character.Init();
                        character.CanMove.SetROri(true);
                        return true;
                    }
                    return false;
                },
                inactivator: (character) =>
                {
                    character.CanMove.SetROri(false);
                    character.IsRemoved.SetROri(true);
                });
            // 池初始化，但是由于服务器采用逐个添加船只的方式，因此这里不进行任何行为
            CharacterPool.Initiate(CharacterType.Tangseng, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Tangseng, MoneyPool));
            CharacterPool.Initiate(CharacterType.Sunwukong, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Sunwukong, MoneyPool));
            CharacterPool.Initiate(CharacterType.Zhubajie, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Zhubajie, MoneyPool));
            CharacterPool.Initiate(CharacterType.Shawujing, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Shawujing, MoneyPool));
            CharacterPool.Initiate(CharacterType.Bailongma, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Bailongma, MoneyPool));
            CharacterPool.Initiate(CharacterType.Monkid, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Monkid, MoneyPool));
            CharacterPool.Initiate(CharacterType.Jiuling, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Jiuling, MoneyPool));
            CharacterPool.Initiate(CharacterType.Honghaier, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Honghaier, MoneyPool));
            CharacterPool.Initiate(CharacterType.Niumowang, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Niumowang, MoneyPool));
            CharacterPool.Initiate(CharacterType.Tieshan, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Tieshan, MoneyPool));
            CharacterPool.Initiate(CharacterType.Zhizhujing, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Zhizhujing, MoneyPool));
            CharacterPool.Initiate(CharacterType.Pawn, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Pawn, MoneyPool));
        }
        public void AddMoney(long add)
        {
            MoneyPool.Money.Add(add);
        }
        public void SubMoney(long sub)
        {
            MoneyPool.Money.SubRNow(sub);
        }
    }
}
