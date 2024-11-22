using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Preparation.Utility.Value.SafeValue.LockedValue;
using Preparation.Utility;
namespace Preparation.Interface
{
    public interface ICharacter : IMovable, IPlayer
    {
        public InVariableRange<long> HP { get; }
        public InVariableRange<long> AttackPower { get; }
        public InVariableRange<long> AttackSize { get; }
        public CharacterType CharacterType { get; }
        public CharacterState CharacterState { get; }
        public long AddMoney(long add);
        public long SubMoney(long sub);
        public long SetCharacterState(CharacterState value = CharacterState.NULL_CHARACTER_STATE, IGameObj? obj = null);
    }
}
