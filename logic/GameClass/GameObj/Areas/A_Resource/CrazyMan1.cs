using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class CrazyMan1 : IAROccupation
{
    public int MaxHp { get; } = GameData.CrazyMan1HP;
    public int AttackPower { get; } = GameData.CrazyMan1ATK;
}
