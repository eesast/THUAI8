using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class LifeMedicine3 : ILifeMedicine
{
    public int Cost => GameData.LifeMedicine3cost;
    public int Recovery => GameData.LifeMedicine3cost;
    public EquipmentType RecoveryType => EquipmentType.LARGE_HEALTH_POTION;
}