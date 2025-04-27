using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Shield3 : IShield
{
    public int Cost => GameData.Shield3cost;
    public int ShieldHP => GameData.Shield3;
    public EquipmentType ShieldType => EquipmentType.LARGE_SHIELD;
}
