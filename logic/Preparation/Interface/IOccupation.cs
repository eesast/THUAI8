﻿using Preparation.Utility;

namespace Preparation.Interface;

public interface IOccupation
{
    public int Cost { get; }
    public int MoveSpeed { get; }
    public int MaxHp { get; }
    public int BaseAttackSize { get; }
    public int ViewRange { get; }
    public int AttackPower { get; }
    public bool IsEquipValid(EquipmentType EquipType);
}
