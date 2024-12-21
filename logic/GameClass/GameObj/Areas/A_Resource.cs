using GameClass.GameObj.Occupations;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class A_Resource
    : Immovable, IA_Recource
{
    public InVariableRange<long> HP { get; }
    public InVariableRange<long> AttackPower { get; }
    public override bool IsRigid(bool args = false) => false;
    protected readonly object actionLock = new();
    public object ActionLock => actionLock;
    public IAROccupation Occupation { get; }
    private AdditionResourceState State = AdditionResourceState.NULL_ADDITION_RESOURCE_STATE;
    public AdditionResourceState ARstate
    {
        get
        {
            lock (actionLock)
                return State;
        }
    }
    public A_ResourceType AResourceType { get; }
    public override ShapeType Shape => ShapeType.NULL_SHAPE_TYPE;
    public void SetARState(AdditionResourceState state)
    {
        State = state;
    }
    public bool TryToRemoveFromGame()
    {
        lock (actionLock)
        {
            TryToRemove();
            position = GameData.PosNotInGame;
        }
        return true;
    }
    public void Init()
    {
        HP.SetMaxV(Occupation.MaxHp);
        HP.SetVToMaxV();
        AttackPower.SetMaxV(Occupation.AttackPower);
        AttackPower.SetVToMaxV();
    }
    public A_Resource(int radius, A_ResourceType type, XY initPos) :
        base(initPos, radius, GameObjType.A_Resource)
    {
        Occupation = ARFactory.FindAROccupation(Type = type);
        HP = new(Occupation.MaxHp);
        AttackPower = new(Occupation.AttackPower);
        Init();
    }
}
