using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos, long teamID)//cage无法被攻击销毁，因此不以construction为基类
    : TrapBase(initPos, teamID)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicBool IsActivated { get; } = new(false);
    public bool SetCage(Character character)
    {
        int constructionspeed = GameData.TrapConstructSpeed;
        return TrapCost.AddVUseOtherRChange<long>(constructionspeed / GameData.NumOfStepPerSecond, character.MoneyPool.Money, 1) > 0;
    }
}
