using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct FillRaycastHitHashMaps : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeArray<RaycastHit> hits;

    public NativeMultiHashMap<int, RaycastHit>.ParallelWriter pollsRaycastHits;

    public int maxHits;

    public void Execute(int index)
    {
        int startIndex = index * maxHits;

        MathUtils.RaycastHitSortByY(ref hits, index * maxHits, maxHits);
        
        for (int i = startIndex; i < startIndex + maxHits && hits[i].normal != default; i++)
        {
            //Places it in the front of the "list"
            pollsRaycastHits.Add(index, hits[i]);
        }
    }
}
