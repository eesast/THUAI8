using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

namespace Preparation.Utility.Value.SafeValue
{
    public static class MyTimerLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("MyTimer");
    }
}

namespace Preparation.Utility.Value.SafeValue.LockedValue
{
    public static class LockedValueLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("LockedValue");
    }
}

namespace Preparation.Utility.Value.SafeValue.TimeBased
{

    public static class TimeBasedLogging
    {
        public static readonly ILogger logger = LoggerF.loggerFactory.CreateLogger("TimeBased");
    }
}