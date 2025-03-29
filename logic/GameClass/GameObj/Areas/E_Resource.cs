using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.LockedValue;
using System.Threading.Tasks.Dataflow;

namespace GameClass.GameObj.Areas;

public class E_Resource(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.ECONOMY_RESOURCE)
{
    public InVariableRange<long> HP { get; } = new(GameData.ResourceHP);
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.NULL_SHAPE_TYPE;
    protected readonly object actionLock = new();
    public object ActionLock => actionLock;
    public bool Produce(int producespeed, Character character)
    {
        return character.MoneyPool.AddMoney(-HP.SubRChange(producespeed)) > 0;
    }
    private EconomyResourceState State = EconomyResourceState.NULL_ECONOMY_RESOURCE_STATE;
    public EconomyResourceState ERstate
    {
        get
        {
            lock (actionLock)
                return State;
        }
    }
    public EconomyResourceType EResourceType { get; }
}