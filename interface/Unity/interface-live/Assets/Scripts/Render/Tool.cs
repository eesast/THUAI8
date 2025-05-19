using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tool
{
    readonly static System.Random a = new();
    public static int GetRandom(int min, int max)
    {
        return a.Next(min, max);
    }

    public static T RandomSelect<T>(IList<T> list)
    {
        return list[GetRandom(0, list.Count)];
    }
    public static Vector2 CellToUxy(int cellx, int celly)
    {
        return new Vector2(0.5f + celly, 49.5f - cellx);
    }
    public static Vector2 GridToUxy(float gridx, float gridy)
    {
        return new Vector2(gridy / 1000, 50 - gridx / 1000);
    }
    // public Vector2 CellToGrid(Vector2 cell)
    // {
    //     return new Vector2(-0.5f + cell.y, 51.5f - cell.x);
    // }
    public static (float, float) UxyToGrid(Vector2 uxy)
    {
        return (1000 * (50 - uxy.y), 1000 * uxy.x);
    }
}
