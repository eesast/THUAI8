using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class LifePool2 : IAROccupation
{
    public int MaxHp { get; } = GameData.LifePool2HP;
    public int AttackPower { get; } = GameData.LifePoolATK;
}
