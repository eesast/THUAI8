using Preparation.Utility.Value.SafeValue.LockedValue;
using Preparation.Utility;
namespace Preparation.Interface
{
    public interface ICharacter : IMovable, IPlayer
    {
        public InVariableRange<long> HP { get; }
        public InVariableRange<long> AttackPower { get; }
        public InVariableRange<long> AttackSize { get; }
        public InVariableRange<long> Shield { get; }
        public InVariableRange<long> Shoes { get; }//移速加成（注意是加成值，实际移速为基础移速+移速加成）
        public CharacterType CharacterType { get; }
        public CharacterState CharacterState1 { get; }//主动状态
        public CharacterState CharacterState2 { get; }//被动状态
        public long AddMoney(long add);
        public long SubMoney(long sub);
        public long SetCharacterState(CharacterState value1 = CharacterState.NULL_CHARACTER_STATE, CharacterState value2 = CharacterState.NULL_CHARACTER_STATE, IGameObj? obj = null);
    }
}
