using Preparation.Interface;
using Preparation.Utility;

namespace GameClass.GameObj.Equipments;
public static class EquipmentFactory
{
    public static int FindCost(EquipmentType type) => type switch
    {
        EquipmentType.SMALL_HEALTH_POTION => GameData.LifeMedicine1cost,
        EquipmentType.MEDIUM_HEALTH_POTION => GameData.LifeMedicine2cost,
        EquipmentType.LARGE_HEALTH_POTION => GameData.LifeMedicine3cost,
        EquipmentType.SMALL_SHIELD => GameData.Shield1cost,
        EquipmentType.MEDIUM_SHIELD => GameData.Shield2cost,
        EquipmentType.LARGE_SHIELD => GameData.Shield3cost,
        EquipmentType.INVISIBILITY_POTION => GameData.InvisibleCost,
        EquipmentType.SPEEDBOOTS => GameData.ShoesCost,
        EquipmentType.BERSERK_POTION => GameData.CrazyCost,
        EquipmentType.PURIFICATION_POTION => GameData.PurificationCost,
        _ => int.MaxValue,
    };
}