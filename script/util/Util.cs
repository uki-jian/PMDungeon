using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
public static class Util
{
    public static int ManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }
}
