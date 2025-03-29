using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public class Space(XY initPos)
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.SPACE)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
}
