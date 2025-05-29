using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class HOLE(XY initPos, long teamID)//trap无法被攻击销毁，因此不以construction为基类
    : TrapBase(initPos, teamID)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicBool IsActivated { get; } = new(false);
    public bool SetHole(Character character)
    {
        int constructionspeed = GameData.TrapConstructSpeed;
        return TrapCost.AddVUseOtherRChange<long>(constructionspeed / GameData.NumOfStepPerSecond, character.MoneyPool.Money, 1) > 0;
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
