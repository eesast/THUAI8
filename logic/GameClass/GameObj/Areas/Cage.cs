using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos)//cage�޷����������٣���˲���constructionΪ����
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);
}
