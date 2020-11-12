using System;
using Unity.Collections;
using UnityEngine;

public struct PointStruct
{
    public Vector3 point;

    //Y = pollPoint / sideCount
    //X = pollPoint % sideCount
    public int pollPoint;

    public PointStruct(Vector3 point, int pollPoint)
    {
        this.point = point;
        this.pollPoint = pollPoint;
    }
}
