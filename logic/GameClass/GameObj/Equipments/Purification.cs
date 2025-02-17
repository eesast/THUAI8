using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Purification : IPurification
{
    public int Cost => GameData.PurificationCost;
    public int ContinueTime => GameData.PurificationTime;
    public EquipmentType equipmentType => EquipmentType.PURIFICATION_POTION;
}
