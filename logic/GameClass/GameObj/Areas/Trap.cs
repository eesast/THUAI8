using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;

namespace GameClass.GameObj.Areas;

public class Trap(XY initPos)//trap无法被攻击销毁，因此不以construction为基类
    : Immovable(initPos, int.MaxValue, GameObjType.NULL)
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID { get; } = new(long.MaxValue);

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
