using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Shoes : IShoes
{
    public int Cost => GameData.ShoesCost;
    public int ShoesSpeed => GameData.ShoesSpeed;
    public EquipmentType ShoesType => EquipmentType.SPEEDBOOTS;
}
