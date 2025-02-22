using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class LifeMedicine1 : ILifeMedicine
{
    public int Cost => GameData.LifeMedicine1cost;
    public int Recovery => GameData.LifeMedicine1cost;
    public EquipmentType RecoveryType => EquipmentType.SMALL_HEALTH_POTION;
}
