using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public class NullArea(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.NULL)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.NULL_SHAPE_TYPE;
}
