using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteract : InteractBase
{
    public enum TileType
    {
        EconomyResource,
        AdditionResource,
        Construction,
        Trap,
    }
    public TileType tileType;
    public Tuple<int, int> pos;
}
