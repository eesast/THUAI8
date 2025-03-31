using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class NullA_Resource : IAROccupation
{
    public int MaxHp { get; } = 0;
    public int AttackPower { get; } = 0;
}
