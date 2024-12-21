using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class E_Resource(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.Null)
{
    public InVariableRange<long> HP { get; } = new(GameData.ResourceHP);
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.NULL_SHAPE_TYPE;
    public bool Produce(int producespeed, Character character)
    {
        return character.MoneyPool.AddMoney(-HP.SubRChange(producespeed)) > 0;
    }
}