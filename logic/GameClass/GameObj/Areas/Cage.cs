using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.Null), Construction
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
}
