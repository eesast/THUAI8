using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue.Atomic;
using Preparation.Utility.Value.SafeValue.LockedValue;

namespace GameClass.GameObj.Areas;

public class HOLE(XY initPos, long teamID)//trap�޷����������٣���˲���constructionΪ����
    : Immovable(initPos, GameData.NumOfPosGridPerCell / 2, GameObjType.TRAP)
{
    public override bool IsRigid(bool args = false) => false;
    public override ShapeType Shape => ShapeType.SQUARE;
    public AtomicLong TeamID = new(teamID);
    public AtomicBool IsActivated { get; } = new(false);
    public InVariableRange<long> HoleCost { get; } = new(0, GameData.TrapCost);//һ��������ֵ�����ڱ�ʾ�޽�����
    public bool SetHole(Character character)
    {
        int constructionspeed = GameData.TrapConstructSpeed;
        return HoleCost.AddVUseOtherRChange<long>(constructionspeed / GameData.NumOfStepPerSecond, character.MoneyPool.Money, 1) > 0;
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
