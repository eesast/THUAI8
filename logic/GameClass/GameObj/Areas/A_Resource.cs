using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class A_Resource(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.Null)
{
    public InVariableRange<long> HP { get; }
    public InVariableRange<long> ATKpower { get; }
    public override bool IsRigid(bool args = false) => false;
    private AdditionResourceState State { get; }
    private A_ResourceType Type { get; }
    public override ShapeType Shape => ShapeType.NULL_SHAPE_TYPE;
}
