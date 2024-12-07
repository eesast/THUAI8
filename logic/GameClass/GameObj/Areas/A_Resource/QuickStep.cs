using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class QuickStep : IAROccupation
{
    public int MaxHp { get; } = GameData.QuickStepHP;
    public int AttackPower { get; } = GameData.QuickStepATK;
}
