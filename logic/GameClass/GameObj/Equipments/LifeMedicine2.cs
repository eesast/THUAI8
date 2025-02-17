using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class LifeMedicine2 : ILifeMedicine
{
    public int Cost => GameData.LifeMedicine2cost;
    public int Recovery => GameData.LifeMedicine2cost;
    public EquipmentType RecoveryType => EquipmentType.MEDIUM_HEALTH_POTION;
}