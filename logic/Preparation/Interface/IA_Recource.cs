using Preparation.Utility.Value.SafeValue.LockedValue;
using Preparation.Utility;

namespace Preparation.Interface
{
    public interface IA_Recource
    {
        public InVariableRange<long> HP { get; }
        public InVariableRange<long> AttackPower { get; }
    }
}
