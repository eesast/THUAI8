using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class Construction(XY initPos)
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.CONSTRUCTION)
{
    public AtomicLong TeamID { get; } = new(long.MaxValue);
    public InVariableRange<long> HP { get; } = new(0, GameData.ConstructionHP);
    public InVariableRange<long> Process { get; } = new(0, GameData.ConstructionProcess);//一个抽象值，用于记录建造进程
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;

    private readonly object lockOfConstructionType = new();
    private ConstructionType constructionType = ConstructionType.NULL_CONSTRUCTION_TYPE;
    public ConstructionType ConstructionType
    {
        get
        {
            lock (lockOfConstructionType)
                return constructionType;
        }
    }
    public AtomicInt ConstructNum { get; } = new(0);
    public AtomicBool IsActivated { get; } = new(false);

    public bool Construct(ConstructionType constructionType, Character character)//这里修改了函数的参数列表删除了int constructSpeed
    {
        int constructSpeed = 0;
        if (constructionType == ConstructionType.NULL_CONSTRUCTION_TYPE)
            return false;
        lock (lockOfConstructionType)
        {
            if (this.constructionType == ConstructionType.NULL_CONSTRUCTION_TYPE || HP == 0)
            {
                TeamID.SetROri(character.TeamID);
                this.constructionType = constructionType;
                switch (constructionType)
                {
                    case ConstructionType.BARRACKS:
                        HP.SetMaxV(GameData.BarracksHP);
                        Process.SetMaxV(GameData.BarracksCost);
                        constructSpeed = GameData.BarracksConstructSpeed;
                        break;

                    case ConstructionType.SPRING:
                        HP.SetMaxV(GameData.SpringHP);
                        constructSpeed = GameData.SpringConstructSpeed;
                        break;
                    case ConstructionType.FARM:
                        HP.SetMaxV(GameData.FarmHP);
                        Process.SetMaxV(GameData.FarmCost);
                        constructSpeed = GameData.FarmConstructSpeed;
                        break;
                    case ConstructionType.HOLE:
                        constructSpeed = GameData.TrapConstructSpeed;
                        break;
                    case ConstructionType.CAGE:
                        constructSpeed = GameData.CageConstructSpeed;
                        break;
                    default:
                        break;
                }
            }
            else
            if (this.constructionType != constructionType)
            {
                return false;
            }
        }
        return Process.AddVUseOtherRChange<long>(GameData.BaseConstructSpeed / GameData.NumOfStepPerSecond, character.MoneyPool.Money, 1) > 0;//原程序constructSpeed的内涵问清楚后再改
    }
    public bool BeAttacked(Character character)
    {
        var previousActivated = IsActivated.Get();
        if (constructionType == ConstructionType.HOLE || constructionType == ConstructionType.CAGE)
        {
            return previousActivated;
        }
        if (character!.TeamID != TeamID)
        {
            long subHP = character.AttackPower;
            HP.SubPositiveV(subHP);
            Process.SubPositiveV(subHP * 20);
        }
        if (HP.IsBelowMaxTimes(0.5))
        {
            IsActivated.Set(false);
        }
        return HP.IsBelowMaxTimes(0.5) && previousActivated;
    }
    public bool BeAttacked(Character character, long AP)
    {
        var previousActivated = IsActivated.Get();
        if (constructionType == ConstructionType.HOLE || constructionType == ConstructionType.CAGE)
        {
            return previousActivated;
        }
        if (character!.TeamID != TeamID)
        {
            long subHP = AP;
            HP.SubPositiveV(subHP);
            Process.SubPositiveV(subHP * 20);
        }
        if (HP.IsBelowMaxTimes(0.5))
        {
            IsActivated.Set(false);
        }
        return HP.IsBelowMaxTimes(0.5) && previousActivated;
    }
    public void AddConstructNum(int add = 1)
    {
        ConstructNum.Add(add);
    }
    public void SubConstructNum(int sub = 1)
    {
        ConstructNum.Sub(sub);
    }
}
