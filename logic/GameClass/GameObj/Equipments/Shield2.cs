using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Shield2 : IShield
{
    public int Cost => GameData.Shield2cost;
    public int ShieldHP => GameData.Shield2;
    public EquipmentType ShieldType => EquipmentType.MEDIUM_SHIELD;
}