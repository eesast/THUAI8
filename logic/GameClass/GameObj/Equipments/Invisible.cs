using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Invisible : IInvisible
{
    public int Cost => GameData.InvisibleCost;
    public int ContinueTime => GameData.InvisibleTime;
    public EquipmentType equipmentType => EquipmentType.INVISIBILITY_POTION;
}