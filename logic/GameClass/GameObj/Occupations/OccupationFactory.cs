using Preparation.Interface;
using Preparation.Utility;

namespace GameClass.GameObj.Occupations
{
    public static class OccupationFactory
    {
        public static IOccupation FindIOccupation(CharacterType charactertype) => charactertype switch
        {
            CharacterType.TangSeng => new TangSeng(),
            CharacterType.SunWukong => new SunWukong(),
            CharacterType.ZhuBajie => new ZhuBajie(),
            CharacterType.ShaWujing => new ShaWujing(),
            CharacterType.BaiLongma => new BaiLongma(),
            CharacterType.Monkid => new Monkid(),
            CharacterType.JiuLing => new JiuLing(),
            CharacterType.HongHaier => new HongHaier(),
            CharacterType.NiuMowang => new NiuMowang(),
            CharacterType.TieShan => new TieShan(),
            CharacterType.ZhiZhujing => new ZhiZhujing(),
            CharacterType.Pawn => new Pawn(),
        };
    }
}
