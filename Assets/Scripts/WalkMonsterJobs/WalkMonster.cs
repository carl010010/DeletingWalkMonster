using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections.Generic;
using RaycastJobs;
using UnityEditor;

[ExecuteAlways]
public class WalkMonster : MonoBehaviour
{
    public float walkMonsterSize = 3;
    public int sideCount = 25;
    public float playerHeight = 2;
    public float stepHeight = 0.9f;
    public float playerRadius = 0.5f;


    [Header("Debug Display")]
    public bool DisplayVaildPoints = false;
    public bool DisplayBlockedPoints = false;
    public bool DisplayWalkPoints = false;
    public bool DisplayFreeWalkPointEdges = false;
    public bool DisplayBlockedWalkPointEdges = false;
    static public bool _DisplayCylinders = false;
    public bool DisplayCylinders = false;

    NativeMultiHashMap<int, RaycastHit> pollsRaycastHits;
    NativeMultiHashMap<int, float> pollsFreePoints;
    NativeMultiHashMap<int, float> pollsBlockedPoints;
    NativeArray<Vector3> pollPositions;

    JobHandle pointsJobHandle;
    GeneratePointsJob pointsJob;
    RaycastAllCommand raycastAllCommand;
    NativeArray<RaycastCommand> raycasts;

    NativeArray<RaycastHit> hits;

    const int maxHits = 10;
    const int maxHeight = 255;
    Material lineMaterial;

    void Awake()
    {
        // must be called before trying to draw lines..
        lineMaterial = GL_Utils.CreateLineMaterial();
        SetUpdateData();
    }

    void OnEnable()
    {
        //Debug.Log("OnEnable!!");
        // must be called before trying to draw lines..
        lineMaterial = GL_Utils.CreateLineMaterial();

        SetUpdateData();
    }

    private void OnDisable()
    {
        DisposeAllPersistentData();

    }

    // Update is called once per frame
    void Update()
    {
        if (walkMonsterSize <= 0.01 || sideCount < 1)
            return;



        //if (hits == null || !hits.IsCreated)
        //{
        //    hits = new NativeArray<RaycastHit>(sideCount * sideCount * maxHits, Allocator.Persistent);
        //}

        if (hits.Length != sideCount * sideCount * maxHits
                        || !hits.IsCreated
                        || !pollsRaycastHits.IsCreated
                        || !pollsFreePoints.IsCreated
                        || !pollsBlockedPoints.IsCreated
                        || !pollPositions.IsCreated
                        || !raycasts.IsCreated)
        {

            DisposeAllPersistentData();
            SetUpdateData();
            return;
        }

        pollsRaycastHits.Clear();
        pollsFreePoints.Clear();
        pollsBlockedPoints.Clear();

        float spacing = (1f / (sideCount - 1)) * walkMonsterSize;

        for (int y = 0; y < sideCount; y++)
        {
            for (int x = 0; x < sideCount; x++)
            {
                Vector3 point = transform.position + new Vector3(0 - (walkMonsterSize / 2) + x * spacing, 0, 0 - (walkMonsterSize / 2) + y * spacing);
                point.y = maxHeight;
                raycasts[y * sideCount + x] = new RaycastCommand(point, Vector3.down);
            }
        }

        raycastAllCommand = new RaycastAllCommand(raycasts, hits, maxHits);
        JobHandle raycastAllCommandJob = raycastAllCommand.Schedule(default);

        raycastAllCommandJob.Complete();
        JobHandle Fill = new FillRaycastHitHashMaps()
        {
            hits = hits,
            maxHits = maxHits,
            pollsRaycastHits = pollsRaycastHits.AsParallelWriter()
        }.Schedule(sideCount * sideCount, 64, raycastAllCommandJob);


        Fill.Complete();

        pointsJob = new GeneratePointsJob()
        {
            pollsRaycastHits = pollsRaycastHits,
            pollsFreePoints = pollsFreePoints.AsParallelWriter(),
            pollsBlockedPoints = pollsBlockedPoints.AsParallelWriter(),
            pollPositions = pollPositions,
            walkMonsterSize = walkMonsterSize,
            sideCount = sideCount,
            playerHeight = playerHeight,
            stepHeight = stepHeight,
            playerRadius = playerRadius,
            pos = transform.position,
            spacing = spacing,
            maxHits = maxHits
        };

        pointsJobHandle = pointsJob.Schedule(sideCount * sideCount, 64, Fill);

        
     }

    private void LateUpdate()
    {
        //pointsJobHandle.Complete();

        pointsJobHandle.Complete();

        


        raycastAllCommand.Dispose();
    }

    void OnRenderObject()
    {
        GL.PushMatrix();

        if (lineMaterial == null)
            lineMaterial = GL_Utils.CreateLineMaterial();

        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);


        for(int i = 0; i < sideCount * sideCount; i++)
        {
            if(pollsFreePoints.TryGetFirstValue(i, out float f, out NativeMultiHashMapIterator<int> it))
            {
                do
                {
                    GL_Utils.DrawCrossVertical(pollPositions[i] + Vector3.up * f + GL_Utils.offset, Color.white);
                }
                while (pollsFreePoints.TryGetNextValue(out f, ref it));
            }
        }

        for (int i = 0; i < sideCount * sideCount; i++)
        {
            if (pollsBlockedPoints.TryGetFirstValue(i, out float f, out NativeMultiHashMapIterator<int> it))
            {
                do
                {
                    GL_Utils.DrawCrossVertical(pollPositions[i] + Vector3.up * f + GL_Utils.offset, Color.red);
                }
                while (pollsBlockedPoints.TryGetNextValue(out f, ref it));
            }
        }

        //foreach (var p in )
        //{
        //    GL_Utils.DrawCrossVertical(p.point + GL_Utils.offset, Color.white);
        //}

        //foreach (var p in blockedPointlist)
        //{
        //    GL_Utils.DrawCrossVertical(p.point + GL_Utils.offset, Color.red);
        //}

        GL.End();
        GL.PopMatrix();
    }


    private void SetUpdateData()
    {
        int size = sideCount * sideCount * maxHits;


        hits = new NativeArray<RaycastHit>(size, Allocator.Persistent);
        pollsRaycastHits = new NativeMultiHashMap<int, RaycastHit>(size, Allocator.Persistent);
        pollsFreePoints = new NativeMultiHashMap<int, float>(size, Allocator.Persistent);
        pollsBlockedPoints = new NativeMultiHashMap<int, float>(size, Allocator.Persistent);
        pollPositions = new NativeArray<Vector3>(sideCount * sideCount, Allocator.Persistent);
        raycasts = new NativeArray<RaycastCommand>(sideCount * sideCount, Allocator.Persistent);
    }

    private void DisposeAllPersistentData()
    {
        if (hits.IsCreated)
            hits.Dispose();

        if (pollsRaycastHits.IsCreated)
            pollsRaycastHits.Dispose();

        if (pollsFreePoints.IsCreated)
            pollsFreePoints.Dispose();

        if (pollsBlockedPoints.IsCreated)
            pollsBlockedPoints.Dispose();

        if (pollPositions.IsCreated)
            pollPositions.Dispose();

        if (raycasts.IsCreated)
            raycasts.Dispose();
    }
}
