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
        public MoneyPool MoneyPool { get; } = new();
        public AtomicInt BarrackNum { get; } = new(0);
        public AtomicInt FarmNum { get; } = new(1);
        public int sideFlag { get; }
        public int MoneyAddPerSecond => FarmNum * GameData.ScoreFarmPerSecond;
        public Base(long teamID, int sideFlag)
        {
            sideFlag = sideFlag;
            TeamID = new(teamID);
            CharacterPool = new(
                classfier: (character) => character.CharacterType,//以下可能出问题
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
            CharacterPool.Initiate(CharacterType.TangSeng, 0,
                              () => new(GameData.CharacterRadius, CharacterType.TangSeng, MoneyPool));
            CharacterPool.Initiate(CharacterType.SunWukong, 0,
                              () => new(GameData.CharacterRadius, CharacterType.SunWukong, MoneyPool));
            CharacterPool.Initiate(CharacterType.ZhuBajie, 0,
                              () => new(GameData.CharacterRadius, CharacterType.ZhuBajie, MoneyPool));
            CharacterPool.Initiate(CharacterType.ShaWujing, 0,
                              () => new(GameData.CharacterRadius, CharacterType.ShaWujing, MoneyPool));
            CharacterPool.Initiate(CharacterType.BaiLongma, 0,
                              () => new(GameData.CharacterRadius, CharacterType.BaiLongma, MoneyPool));
            CharacterPool.Initiate(CharacterType.Monkid, 0,
                              () => new(GameData.CharacterRadius, CharacterType.Monkid, MoneyPool));
            CharacterPool.Initiate(CharacterType.JiuLing, 0,
                              () => new(GameData.CharacterRadius, CharacterType.JiuLing, MoneyPool));
            CharacterPool.Initiate(CharacterType.HongHaier, 0,
                              () => new(GameData.CharacterRadius, CharacterType.HongHaier, MoneyPool));
            CharacterPool.Initiate(CharacterType.NiuMowang, 0,
                              () => new(GameData.CharacterRadius, CharacterType.NiuMowang, MoneyPool));
            CharacterPool.Initiate(CharacterType.TieShan, 0,
                              () => new(GameData.CharacterRadius, CharacterType.TieShan, MoneyPool));
            CharacterPool.Initiate(CharacterType.ZhiZhujing, 0,
                              () => new(GameData.CharacterRadius, CharacterType.ZhiZhujing, MoneyPool));
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
