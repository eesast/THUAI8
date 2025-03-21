using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos)//cage�޷����������٣���˲���constructionΪ����
    : Immovable(initPos, int.MaxValue, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);
}
