using Preparation.Utility;
using Preparation.Utility.Value;

namespace GameClass.GameObj.Areas;

public static class AreaFactory
{
    public static Immovable GetArea(XY pos, PlaceType placeType, A_ResourceType type = A_ResourceType.NULL) => placeType switch
    {
        //PlaceType.Home => new Home(pos),
        PlaceType.BARRIER => new Barriers(pos),
        PlaceType.BUSH => new Bush(pos),
        //PlaceType.TRAP => new Trap(pos),
        PlaceType.ECONOMY_RESOURCE => new E_Resource(pos),
        PlaceType.CONSTRUCTION => new Construction(pos),
        PlaceType.SPACE => new Space(pos),
        PlaceType.ADDITION_RESOURCE => new A_Resource(GameData.NumOfPosGridPerCell / 2, type, pos),
        //PlaceType.Wormhole => new Wormhole(pos),
        _ => new NullArea(pos)
    };
    public static OutOfBoundBlock GetOutOfBoundBlock(XY pos) => new(pos);
}
