using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class TrapBase(XY initPos, long teamID)
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID = new(teamID);
    public InVariableRange<long> TrapCost { get; } = new(0, GameData.TrapCost);//一个抽象数值，用于表示修建过程
}

