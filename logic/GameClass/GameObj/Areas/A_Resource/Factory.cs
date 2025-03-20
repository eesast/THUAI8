using Preparation.Interface;
using Preparation.Utility;

namespace GameClass.GameObj.Areas;

public static class ARFactory
{
    public static IAROccupation FindAROccupation(A_ResourceType type) => type switch
    {
        A_ResourceType.CRAZY_MAN1 => new CrazyMan1(),
        A_ResourceType.CRAZY_MAN2 => new CrazyMan2(),
        A_ResourceType.CRAZY_MAN3 => new CrazyMan3(),
        A_ResourceType.LIFE_POOL1 => new LifePool1(),
        A_ResourceType.LIFE_POOL2 => new LifePool2(),
        A_ResourceType.LIFE_POOL3 => new LifePool3(),
        A_ResourceType.QUICK_STEP => new QuickStep(),
        A_ResourceType.WIDE_VIEW => new WideView(),
    };
}