using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class CrazyMan2 : IAROccupation
{
    public int MaxHp { get; } = GameData.CrazyMan2HP;
    public int AttackPower { get; } = GameData.CrazyMan2ATK;
}