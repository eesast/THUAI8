using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class Cage(XY initPos)//cage无法被攻击销毁，因此不以construction为基类
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);
    public AtomicBool IsActivated { get; } = new(false);
    public InVariableRange<long> CageCost { get; } = new(0, GameData.TrapCost);//一个抽象数值，用于表示修建过程
    public bool SetCage(Character character)
    {
        int constructionspeed = GameData.TrapConstructSpeed;
        TeamID.SetROri(character.TeamID);
        return CageCost.AddVUseOtherRChange<long>(constructionspeed, character.MoneyPool.Money, 1) > 1;
    }
}
