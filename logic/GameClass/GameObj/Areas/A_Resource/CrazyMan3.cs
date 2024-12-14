using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class CrazyMan3 : IAROccupation
{
    public int MaxHp { get; } = GameData.CrazyMan3HP;
    public int AttackPower { get; } = GameData.CrazyMan3ATK;
}
