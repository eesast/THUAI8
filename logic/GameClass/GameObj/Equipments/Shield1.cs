using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Shield1 : IShield
{
    public int Cost => GameData.Shield1cost;
    public int ShieldHP => GameData.Shield1;
    public EquipmentType ShieldType => EquipmentType.SMALL_SHIELD;
}
