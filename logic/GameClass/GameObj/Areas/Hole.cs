using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class HOLE(XY initPos)//trap无法被攻击销毁，因此不以construction为基类
    : Immovable(initPos, int.MaxValue, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);
    public AtomicBool IsActivated { get; } = new(false);
    public InVariableRange<long> HoleCost { get; } = new(0, GameData.TrapCost);//一个抽象数值，用于表示修建过程
    public bool SetHole(Character character)
    {
        int constructionspeed = GameData.TrapConstructSpeed;
        TeamID.SetROri(character.TeamID);
        return HoleCost.AddVUseOtherRChange<long>(constructionspeed, character.MoneyPool.Money, 1) > 1;
    }
    /*public bool InSquare(Character character, int range)
    {
        return character.Pos.x >= Pos.x - range && character.Pos.x <= Pos.x + range && character.Pos.y >= Pos.y - range && character.Pos.y <= Pos.y + range;
    }
    public void TrapCharacter(Character character)
    {
        if (CharacterIn(character, GameData.TrapRange))
        {
            character.HP.SubV(GameData.TrapDamage);
            character.CharacterState1 = CharacterState.STUNNED;
        }
    }*/
}
