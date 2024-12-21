using Preparation.Interface;
using Preparation.Utility;

namespace GameClass.GameObj.Areas;

public static class ARFactory
{
    public static IAROccupation FindAROccupation(A_ResourceType type) => type switch
    {
        A_ResourceType.CrazyMan1 => new CrazyMan1(),
        A_ResourceType.CrazyMan2 => new CrazyMan2(),
        A_ResourceType.CrazyMan3 => new CrazyMan3(),
        A_ResourceType.LifePool1 => new LifePool1(),
        A_ResourceType.LifePool2 => new LifePool2(),
        A_ResourceType.LifePool3 => new LifePool3(),
        A_ResourceType.QuickStep => new QuickStep(),
        A_ResourceType.WideView => new WideView(),
    };
}