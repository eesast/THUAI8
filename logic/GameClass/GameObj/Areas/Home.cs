using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class Home(XY initPos, long id, int sideFlag)
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.Home), IHome
{
    public long TeamID { get; } = id;
    public int sideFlag = sideFlag;
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
}
