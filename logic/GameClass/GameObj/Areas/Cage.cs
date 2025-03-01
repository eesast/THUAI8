using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos)//cage无法被攻击销毁，因此不以construction为基类
    : Immovable(initPos, int.MaxValue, GameObjType.Null)
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);
}
