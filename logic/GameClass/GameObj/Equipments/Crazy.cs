using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Equipments;
public class Crazy : ICrazy
{
    public int Cost => GameData.CrazyCost;
    public int ContinueTime => GameData.CrazyTime;
    public double PowerImprove => GameData.CrazyPower;
    public int SpeedImprove => GameData.CrazySpeed;
    public double AttackFrequencyImprove => GameData.CrazyATKFreq;
    public EquipmentType equipmentType => EquipmentType.BERSERK_POTION;
}
