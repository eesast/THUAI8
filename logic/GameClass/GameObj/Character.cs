using GameClass.GameObj.Occupations;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;
using GameClass.GameObj.Areas;

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
    public bool visbility { get; set; } = true;
    public bool trapped { get; set; } = false;
    public bool caged { get; set; } = false;
    public bool stunned { get; set; } = false;
    public double HarmCut = 0.0;//伤害减免，该值范围为0-1，为比例减伤。
    private Timer? trapTimer = null;
    private Timer? cageTimer = null;
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
    public IMoneyPool MoneyPool { get; }
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
                    break;
                case CharacterState.STUNNED://被定身时无法移动
                    if (value1 == CharacterState.MOVING)
                        return -1;
                    else
                        return ChangeCharacterState(value1, value2, gameobj);
                    break;
                case CharacterState.KNOCKED_BACK://击退时无法进行任何操作
                    return -1;
                    break;
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
        AttackPower.SetMaxV(Occupation.AttackPower);
        AttackPower.SetVToMaxV();
    }
    public Character(int radius, CharacterType type, MoneyPool pool) :
        base(GameData.PosNotInGame, radius, GameObjType.Character)
    {
        CanMove.SetROri(false);
        IsRemoved.SetROri(true);
        Occupation = OccupationFactory.FindIOccupation(CharacterType = type);
        ViewRange = Occupation.ViewRange;
        HP = new(Occupation.MaxHp);
        AttackPower = new(Occupation.AttackPower);
        AttackSize = new(Occupation.BaseAttackSize);
        MoneyPool = pool;
        Init();
    }
    public bool InSquare(XY pos, int range)
    {
        return pos.x >= Position.x - range && pos.x <= Position.x + range && pos.y >= Position.y - range && pos.y <= Position.y + range;
    }
    public void InTrap(Trap trap)
    {
        if (!trapped && InSquare(trap.Position, GameData.TrapRange) && trap.TeamID != TeamID)
        {
            visbility = true;
            trapped = true;
            StartTrapTimer(trap);
            //HP.SubV(GameData.TrapDamage);
            //SetCharacterState(CharacterState.STUNNED);

        }
    }
    private void StartTrapTimer(Trap trap)
    {
        StopTrapTimer();
        trapTimer = new Timer(GameData.TimerInterval);
        int elapsedSeconds = 0;
        trapTimer.Elapsed += (sender, e) =>
        {
            HP.SubV(GameData.TrapDamage);//如果造成伤害要改在这里改
            elapsedSeconds++;
            if (elapsedSeconds >= GameData.TrapTime / 1000)
            {
                trapped = false;
                StopTrapTimer();
            }
        };
        trapTimer.AutoReset = false;
        trapTimer.Enabled = true;
    }
    public void StopTrapTimer()
    {
        if (trapTimer != null)
        {
            trapTimer.Stop();
            trapTimer.Dispose();
            trapTimer = null;
        }
    }
    public void InCage(Cage cage)
    {
        if (!caged && InSquare(trap.Pos, GameData.TrapRange) && cage.TeamID != TeamID)
        {
            visbility = true;
            caged = true;
            stunned = true;
            StartCageTimer(trap);
            //HP.SubV(GameData.TrapDamage);
            //SetCharacterState(CharacterState.STUNNED);

        }
    }
    private void StartCageTimer(Trap trap)
    {
        StopCageTimer();
        cageTimer = new Timer(GameData.TimerInterval);
        int elapsedSeconds = 0;
        trapTimer.Elapsed += (sender, e) =>
        {

            elapsedSeconds++;
            if (elapsedSeconds >= GameData.TrapTime / 1000)
            {
                caged = false;
                stunned = false;
                StopCageTimer();
            }
        };
        cageTimer.AutoReset = false;
        cageTimer.Enabled = true;
    }
    public void StopCageTimer()
    {
        if (cageTimer != null)
        {
            cageTimer.Stop();
            cageTimer.Dispose();
            cageTimer = null;
        }
    }
}


