using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct DisposeArray<T> : IJobParallelFor where T : struct
{
    [WriteOnly]
    public NativeHashMap<int, NativeList<T>> DisposeInnerNativeContainer;

    public void Execute(int index)
    {
        if (DisposeInnerNativeContainer.TryGetValue(index, out NativeList<T> o))
            o.Dispose();
    }
}