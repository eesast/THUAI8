using System;
using System.Collections.Generic;
using UnityEngine;
using Protobuf;

public class TileInteract : InteractBase
{
    public enum TileType
    {
        Home,
        Barracks,
        Farm,
        EconomyResource,
        AdditionResource,
        Trap,
    }
    public TileType tileType;
    public Tuple<int, int> pos;
    private int lastHp;
    public object GetMessage() => tileType switch
    {
        TileType.Barracks => CoreParam.barracks.GetValueOrDefault(pos, null),
        TileType.Farm => CoreParam.farms.GetValueOrDefault(pos, null),
        TileType.EconomyResource => CoreParam.economyResources.GetValueOrDefault(pos, null),
        TileType.AdditionResource => CoreParam.additionResources.GetValueOrDefault(pos, null),
        TileType.Trap => CoreParam.traps.GetValueOrDefault(pos, null),
        _ => null,
    };
    public int GetHP() => tileType switch
    {
        TileType.Barracks => CoreParam.barracks.GetValueOrDefault(pos, null)?.Hp ?? 0,
        TileType.Farm => CoreParam.farms.GetValueOrDefault(pos, null)?.Hp ?? 0,
        TileType.EconomyResource => GetMaxHP() - CoreParam.economyResources.GetValueOrDefault(pos, null)?.Process ?? 0,
        TileType.AdditionResource => CoreParam.additionResources.GetValueOrDefault(pos, null)?.Hp ?? 0,
        TileType.Trap => -1,
        _ => 0,
    };
    public int GetMaxHP() => tileType switch
    {
        TileType.Barracks => ParaDefine.Instance.GetData(ConstructionType.Barracks).maxHp,
        TileType.Farm => ParaDefine.Instance.GetData(ConstructionType.Farm).maxHp,
        TileType.EconomyResource => 100,
        TileType.AdditionResource => ParaDefine.Instance.GetData(CoreParam.additionResources[pos].AdditionResourceType).maxHp,
        TileType.Trap => -1,
        _ => 0,
    };

    void Start()
    {
        var hpBar = GetComponentInChildren<HpBar>();
        if (hpBar != null)
        {
            hpBar.team = tileType switch
            {
                TileType.Barracks => CoreParam.barracks[pos].TeamId == 0 ? PlayerTeam.BuddhistsTeam : PlayerTeam.MonstersTeam,
                TileType.Farm => CoreParam.farms[pos].TeamId == 0 ? PlayerTeam.BuddhistsTeam : PlayerTeam.MonstersTeam,
                _ => PlayerTeam.NullTeam
            };
            hpBar.getHp = GetHP;
            hpBar.getMaxHp = GetMaxHP;
        }
    }

    void Update()
    {
        if (tileType != TileType.Barracks && tileType != TileType.Farm && tileType != TileType.AdditionResource) return;
        int harm = lastHp - GetHP();
        if (harm > 0)
        {
            foreach (var (id, message) in CoreParam.characters)
            {
                int atk = message.CommonAttack;
                int range = message.CommonAttackRange;
                int sqDist = (int)Mathf.Pow(message.X - pos.Item1, 2) + (int)Mathf.Pow(message.Y - pos.Item2, 2);
                if (atk == harm && sqDist <= range * range)
                {
                    CharacterManager.Instance.characterInfo[id].characterBase.ManualSetAttack();
                    break;
                }
            }
        }
        lastHp = GetHP();
    }


}
