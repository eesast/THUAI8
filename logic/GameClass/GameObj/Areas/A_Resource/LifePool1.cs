using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class LifePool1 : IAROccupation
{
    public int MaxHp { get; } = GameData.LifePool1HP;
    public int AttackPower { get; } = GameData.LifePoolATK;
}
