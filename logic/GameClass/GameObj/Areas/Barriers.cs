using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public class Barriers(XY initPos)//为了避免不明确引用，命名从Barrier改为Barriers
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.BARRIER)
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
}