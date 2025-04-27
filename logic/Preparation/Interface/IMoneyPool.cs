using Preparation.Utility.Value.SafeValue.Atomic;

namespace Preparation.Interface
{
    public interface IMoneyPool
    {
        public AtomicLong Score { get; }
        public long AddMoney(long add);
        public long SubMoney(long sub);
    }
}
