using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class WideView : IAROccupation
{
    public int MaxHp { get; } = GameData.WideViewHP;
    public int AttackPower { get; } = GameData.WideViewATK;
}
