using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public class Trap(XY initPos)
    : Immovable(initPos, int.MaxValue, GameObjType.Null), Construction
{
    public override bool IsRigid(bool args = false) => true;
    public override ShapeType Shape => ShapeType.SQUARE;

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
