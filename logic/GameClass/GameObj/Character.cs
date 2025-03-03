using GameClass.GameObj.Occupations;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;
using GameClass.GameObj.Areas;
using GameClass.GameObj.Equipments;
using System.Timers;

namespace GameClass.GameObj;
public class Character : Movable, ICharacter
{
    public AtomicLong TeamID { get; } = new(long.MaxValue);
    public AtomicLong PlayerID { get; } = new(long.MaxValue);
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.CIRCLE;
    public int ViewRange { get; }
    public InVariableRange<long> HP { get; }
    public InVariableRange<long> AttackPower { get; }
    public InVariableRange<long> AttackSize { get; }
    public InVariableRange<long> Shield { get; }
    public InVariableRange<long> Shoes { get; }//移速加成（注意是加成值，实际移速为基础移速+移速加成）
    public CharacterType CharacterType { get; }
    public bool trapped { get; set; } = false;
    public bool caged { get; set; } = false;
    public bool stunned { get; set; } = false;
    public bool burned { get; set; } = false;
    public bool visible { get; set; } = true;
    public bool blind { get; set; } = false;
    public double HarmCut = 0.0;//伤害减免，该值范围为0-1，为比例减伤。
    public double ATKFrequency = 1.0;//攻击频率，即每秒攻击次数。
    public long TrapTime = long.MaxValue;
    public long CageTime = long.MaxValue;
    public long BurnedTime = long.MaxValue;
    public long BlindTime = long.MaxValue;
    public long StunnedTime = long.MaxValue;
    public long HarmCutTime = long.MaxValue;
    private long skillCD = long.MaxValue;
    public bool canskill = true;
    public void StartSkillCD()
    {
        skillCD = Environment.TickCount64;
    }
    public void ResetSkillCD()
    {
        skillCD = long.MaxValue;
    }
    public long GetSkillTime()
    {
        return skillCD;
    }
    private CharacterState characterState1 = CharacterState.NULL_CHARACTER_STATE;
    private CharacterState characterState2 = CharacterState.DECEASED;
    public CharacterState CharacterState1
    {
        get
        {
            lock (actionLock)
                return characterState1;
        }
    }
    public CharacterState CharacterState2
    {
        get
        {
            lock (actionLock)
                return characterState2;
        }
    }
    public IOccupation Occupation { get; }
    public MoneyPool MoneyPool { get; }
    private GameObj? InteractObj = null;
    public GameObj? GetInteractObj
    {
        get
        {
            lock (actionLock)
            {
                return InteractObj;
            }
        }
    }
    public long AddMoney(long add)
    {
        return MoneyPool.AddMoney(add);
    }
    public long SubMoney(long sub)
    {
        return MoneyPool.SubMoney(sub);
    }
    public override bool IgnoreCollideExecutor(IGameObj targetObj)
    {
        if (IsRemoved)
            return true;
        if (targetObj.Type == GameObjType.Character
         && XY.DistanceCeil3(targetObj.Position, Position)
            < Radius + targetObj.Radius - GameData.AdjustLength)
            return true;
        return false;
    }
    public long GetCost()
    {
        var cost = 0;
        switch (CharacterType)
        {
            case CharacterType.TangSeng:
                cost += 0;
                break;
            case CharacterType.JiuLing:
                cost += 0;
                break;
            case CharacterType.SunWukong:
                cost += GameData.SunWukongcost;
                break;
            case CharacterType.ZhuBajie:
                cost += GameData.ZhuBajiecost;
                break;
            case CharacterType.ShaWujing:
                cost += GameData.ShaWujingcost;
                break;
            case CharacterType.BaiLongma:
                cost += GameData.BaiLongmacost;
                break;
            case CharacterType.Monkid:
                cost += GameData.Monkidcost;
                break;
            case CharacterType.HongHaier:
                cost += GameData.HongHaiercost;
                break;
            case CharacterType.NiuMowang:
                cost += GameData.NiuMowangcost;
                break;
            case CharacterType.TieShan:
                cost += GameData.TieShancost;
                break;
            case CharacterType.ZhiZhujing:
                cost += GameData.ZhiZhujingcost;
                break;
            case CharacterType.Pawn:
                cost += GameData.Pawncost;
                break;
        }
        return cost;
    }
    private long ChangeCharacterState(CharacterState value1 = CharacterState.NULL_CHARACTER_STATE, CharacterState value2 = CharacterState.NULL_CHARACTER_STATE, GameObj? gameobj = null)
    {
        //只能被SetCharacterState引用
        InteractObj = gameobj;
        characterState1 = value1;
        characterState2 = value2;
        return stateNum;
    }
    public long SetCharacterState(CharacterState value1 = CharacterState.NULL_CHARACTER_STATE, CharacterState value2 = CharacterState.NULL_CHARACTER_STATE, IGameObj? obj = null)
    {
        GameObj? gameobj = (GameObj?)obj;
        lock (actionLock)
        {
            CharacterState nowState1 = characterState1;
            CharacterState nowState2 = characterState2;
            if (nowState1 == value1 && nowState2 == value2) return -1;
            if (value2 == CharacterState.NULL_CHARACTER_STATE)
                value2 = nowState2;
            //此部分代码存在问题需要解决：当角色通过商店等获取新的被动状态时，原有的被动状态会因此被覆盖失效
            switch (nowState2)
            {
                case CharacterState.BLIND://致盲时无法攻击或使用技能
                    if (value1 == CharacterState.ATTACKING || value1 == CharacterState.SKILL_CASTING)
                        return -1;
                    else
                        return ChangeCharacterState(value1, value2, gameobj);
                case CharacterState.STUNNED://被定身时无法移动
                    if (value1 == CharacterState.MOVING)
                        return -1;
                    else
                        return ChangeCharacterState(value1, value2, gameobj);
                case CharacterState.KNOCKED_BACK://击退时无法进行任何操作
                    return -1;
                default:
                    return ChangeCharacterState(value1, value2, gameobj);
            }
        }
    }
    public bool Commandable()
    {
        lock (ActionLock)
        {
            return (characterState2 != CharacterState.KNOCKED_BACK);
        }
    }

    public bool StartThread(long stateNum)
    {
        lock (actionLock)
        {
            if (StateNum == stateNum)
            {
                CharacterLogging.logger.ConsoleLogDebug(
                    LoggingFunctional.CharacterLogInfo(this)
                    + " StartThread succeeded");
                return true;
            }
        }
        CharacterLogging.logger.ConsoleLogDebug(
            LoggingFunctional.CharacterLogInfo(this)
            + " StartThread failed");
        return false;
    }

    public bool TryToRemoveFromGame(CharacterState state)
    {
        lock (actionLock)
        {
            if (SetCharacterState(CharacterState.NULL_CHARACTER_STATE, state) == -1)
                return false;
            TryToRemove();
            CanMove.SetROri(false);
            position = GameData.PosNotInGame;
        }
        return true;
    }
    public void Init()
    {
        HP.SetMaxV(Occupation.MaxHp);
        HP.SetVToMaxV();
    }
    public Character(int radius, CharacterType type, MoneyPool pool) :
        base(GameData.PosNotInGame, radius, GameObjType.Character)
    {
        CanMove.SetROri(false);
        IsRemoved.SetROri(true);
        Occupation = OccupationFactory.FindIOccupation(CharacterType = type);
        ViewRange = Occupation.ViewRange;
        Shoes = new(0);
        Shield = new(0);
        AttackSize = new(Occupation.BaseAttackSize);
        AttackPower = new(Occupation.AttackPower);
        MoneyPool = pool;
        Init();
    }
    public bool InSquare(XY pos, int range)
    {
        return pos.x >= Position.x - range && pos.x <= Position.x + range && pos.y >= Position.y - range && pos.y <= Position.y + range;
    }
    public bool GetEquipments(EquipmentType equiptype)
    {
        if (equiptype == EquipmentType.NULL_EQUIPMENT_TYPE) return false;
        if (!Occupation.IsEquipValid(equiptype)) return false;
        if (MoneyPool.Money < EquipmentFactory.FindCost(equiptype)) return false;
        switch (equiptype)
        {
            case EquipmentType.SMALL_HEALTH_POTION:
                {
                    HP.AddPositiveV(GameData.LifeMedicine1HP);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.MEDIUM_HEALTH_POTION:
                {
                    HP.AddPositiveV(GameData.LifeMedicine2HP);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.LARGE_HEALTH_POTION:
                {
                    HP.AddPositiveV(GameData.LifeMedicine3HP);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.SMALL_SHIELD:
                {
                    Shield.AddPositiveV(GameData.Shield1);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.MEDIUM_SHIELD:
                {
                    Shield.AddPositiveV(GameData.Shield2);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.LARGE_SHIELD:
                {
                    Shield.AddPositiveV(GameData.Shield3);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.SPEEDBOOTS:
                {
                    Shoes.AddPositiveV(GameData.ShoesSpeed);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.INVISIBILITY_POTION:
                {
                    SetCharacterState(CharacterState1, CharacterState.INVISIBLE);//此处缺少时间限制
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            case EquipmentType.BERSERK_POTION:
                {
                    SetCharacterState(CharacterState1, CharacterState.BERSERK);//此处缺少时间限制
                    AttackPower.AddPositiveV((long)(0.2 * AttackPower.GetValue()));
                    ATKFrequency = GameData.CrazyATKFreq;
                    Shoes.AddPositiveV(GameData.CrazySpeed);
                    SubMoney(EquipmentFactory.FindCost(equiptype));
                    return true;
                }
            default: return false;
        }
    }
}


