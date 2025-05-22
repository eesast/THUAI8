using Microsoft.Extensions.Logging;
using Preparation.Utility.Logging;

//namespace Preparation.Utility.Value.SafeValue
//{
//    public static class LogicLogging
//    {
//        public static readonly ILogger logger = AdvancedLoggerFactory.loggerFactory.CreateLogger("MyTimer");
//    }
//}

//namespace Preparation.Utility.Value.SafeValue.LockedValue
//{
//    public static class LogicLogging
//    {
//        public static readonly ILogger logger = AdvancedLoggerFactory.loggerFactory.CreateLogger("LockedValue");
//    }
//}

//namespace Preparation.Utility.Value.SafeValue.TimeBased
//{

//    public static class LogicLogging
//    {
//        public static readonly ILogger logger = AdvancedLoggerFactory.loggerFactory.CreateLogger("TimeBased");
//    }
//}

namespace Preparation.Utility
{
    public static class LogicLogging
    {
        public static readonly AdvancedLoggerFactory.Logger logger = AdvancedLoggerFactory.CreateLogger("Logic");
    }
}