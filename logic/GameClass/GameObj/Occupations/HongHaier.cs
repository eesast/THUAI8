using Preparation.Interface;
using Preparation.Utility;


namespace GameClass.GameObj.Occupations
{
    public class HongHaier : IOccupation
    {
        public int MoveSpeed { get; } = GameData.NumOfStepPerSecond;
        public int MaxHp { get; } = GameData.HongHaierHP;
        public int ViewRange { get; } = GameData.Viewrange;
        public int Cost { get; } = GameData.HongHaiercost;
        public int BaseAttackSize { get; } = GameData.HongHaierATKsize;
        public int AttackPower { get; } = GameData.HongHaierATKpower;
        public bool IsEquipValid(EquipmentType equiptype) => equiptype switch
        {
            EquipmentType.SPEEDBOOTS => false,
            EquipmentType.SMALL_HEALTH_POTION => false,
            EquipmentType.MEDIUM_HEALTH_POTION => false,
            EquipmentType.LARGE_HEALTH_POTION => false,

            EquipmentType.SMALL_SHIELD => false,
            EquipmentType.MEDIUM_SHIELD => false,
            EquipmentType.LARGE_SHIELD => false,

            EquipmentType.BERSERK_POTION => false,

            EquipmentType.INVISIBILITY_POTION => false,

            _ => false
        };
    }
}
