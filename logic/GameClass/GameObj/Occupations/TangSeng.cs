﻿using Preparation.Interface;
using Preparation.Utility;

namespace GameClass.GameObj.Occupations
{
    public class TangSeng : IOccupation
    {
        public int MoveSpeed { get; } = GameData.NumOfStepPerSecond;
        public int MaxHp { get; } = GameData.TangSengHP;
        public int ViewRange { get; } = GameData.Viewrange;
        public int Cost { get; } = 0;
        public int BaseAttackSize { get; } = GameData.TangSengATKsize;
        public int AttackPower { get; } = GameData.TangSengATKpower;
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
