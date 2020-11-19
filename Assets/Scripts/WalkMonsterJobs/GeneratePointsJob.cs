using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;



[BurstCompile]
public struct GeneratePointsJob : IJobParallelFor
{
    [ReadOnly]
    public NativeMultiHashMap<int, RaycastHit> pollsRaycastHits;
    [WriteOnly]
    public NativeMultiHashMap<int, float>.ParallelWriter pollsFreePoints;
    [WriteOnly]
    public NativeMultiHashMap<int, float>.ParallelWriter pollsBlockedPoints;
    [WriteOnly]
    public NativeArray<Vector3> pollPositions;

    public float walkMonsterSize;
    public int sideCount;
    public float playerHeight;
    public float stepHeight;
    public float playerRadius;

    public Vector3 pos;
    public float spacing;

    public int maxHits;

    public void Execute(int index)
    {

        int y = index / sideCount;
        int x = index % sideCount;

        Vector3 point = pos + new Vector3(0 - (walkMonsterSize / 2) + x * spacing, 0, 0 - (walkMonsterSize / 2) + y * spacing);
        //Y value must be set to zero because of the way Fill walk points works in WalkCell class
        point.y = 0;

        pollPositions[index] = point;

        NativeList<float> freePointList = new NativeList<float>(maxHits, Allocator.Temp);
        NativeList<float> blockedPointList = new NativeList<float>(maxHits, Allocator.Temp);

        FindValidPoints(index, freePointList, blockedPointList, pollsRaycastHits);
        RemoveCloseValues(freePointList, blockedPointList, stepHeight * 0.9f);

        for(int i = 0; i < freePointList.Length; i++)
        {
            pollsFreePoints.Add(index, freePointList[i]);
        }

        for (int i = 0; i < blockedPointList.Length; i++)
        {
            pollsBlockedPoints.Add(index, blockedPointList[i]);
        }

        freePointList.Dispose();
        blockedPointList.Dispose();
    }


    private void FindValidPoints(int index, NativeList<float> freePointList,
                                            NativeList<float> blockedPointList,
                                            NativeMultiHashMap<int, RaycastHit> hits)
    {
        
        if(hits.ContainsKey(index))
        {
            NativeMultiHashMap<int, RaycastHit>.Enumerator tempHits = hits.GetValuesForKey(index);
            tempHits.MoveNext();

            //The Top most point is always free
            freePointList.Add(tempHits.Current.point.y);
            Vector3 prevPoint = tempHits.Current.point;

            while(tempHits.MoveNext())
            {
                RaycastHit currentHit = tempHits.Current;

                if (currentHit.point.y + playerHeight > prevPoint.y)
                {
                    blockedPointList.Add(currentHit.point.y);
                }
                else
                {
                    freePointList.Add(currentHit.point.y);
                }

                prevPoint = currentHit.point;
            }

        }
    }

    private void RemoveCloseValues(NativeList<float> freeYHeights, NativeList<float> blockedYHeights, float v)
    {
        for (int i = 0; i < freeYHeights.Length; i++)
        {
            for (int j = blockedYHeights.Length - 1; j >= 0; j--)
            {
                if (Mathf.Abs(freeYHeights[i] - blockedYHeights[j]) < v)
                    blockedYHeights.RemoveAt(j);
            }
        }
    }
}

