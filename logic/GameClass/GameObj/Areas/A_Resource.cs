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
    public override bool IsRigid(bool args = false) => true;
    public int refreshCount = 0;
    public bool refresh_at_3_min = false;
    public bool refresh_at_7_min = false;
    protected readonly object actionLock = new();
    public object ActionLock => actionLock;
    public long LastAttackTime = 0;
    public long LastBeAttackedTime = 0;
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
    public A_ResourceType AResourceType { get; set; }
    public override ShapeType Shape => ShapeType.SQUARE;
    public void SetARState(AdditionResourceState state)
    {
        State = state;
    }
    public void SetARtype(A_ResourceType type)
    {
        AResourceType = type;
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
        SetARState(AdditionResourceState.BEATABLE);
    }
    public A_Resource(int radius, A_ResourceType type, XY initPos) :
        base(initPos, radius, GameObjType.ADDITIONAL_RESOURCE)
    {
        Occupation = ARFactory.FindAROccupation(type);
        AResourceType = type;
        HP = new(Occupation.MaxHp);
        AttackPower = new(Occupation.AttackPower);
        Init();
    }
}
