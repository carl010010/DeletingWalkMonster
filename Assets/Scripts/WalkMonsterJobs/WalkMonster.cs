using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System;
using System.Collections.Generic;
using RaycastJobs;

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


    NativeQueue<PointStruct> freePoints;
    NativeQueue<PointStruct> blockedPoints;


    //NativeArray<PointStruct> polls;
    JobHandle pollsJobHandle;
    GeneratePolls pollsJob;
    RaycastAllCommand raycastAllCommand;
    NativeArray<RaycastCommand> raycasts;

    NativeArray<RaycastHit> hits;
    JobHandle raycastAllCommandJob;

    const int maxHits = 10;
    const int maxHeight = 255;
    Material lineMaterial;

    void Awake()
    {
        // must be called before trying to draw lines..
        lineMaterial = GL_Utils.CreateLineMaterial();
        //polls = new NativeArray<PointStruct>(sideCount * sideCount, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        //polls.Dispose();
        if (hits != null && hits.IsCreated)
        {
            hits.Dispose();
        }
    }

    private void OnDisable()
    {
        if (hits != null && hits.IsCreated)
        {
            hits.Dispose();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (walkMonsterSize <= 0.01 || sideCount < 1)
            return;



        if (hits == null || !hits.IsCreated)
        {
            hits = new NativeArray<RaycastHit>(sideCount * sideCount * maxHits, Allocator.Persistent);
        }

        if (hits.Length != sideCount * sideCount * maxHits)
        {
            hits.Dispose();
            hits = new NativeArray<RaycastHit>(sideCount * sideCount * maxHits, Allocator.Persistent);
        }

        raycasts = new NativeArray<RaycastCommand>(sideCount * sideCount, Allocator.TempJob);

        float spacing = (1f / (sideCount - 1)) * walkMonsterSize;

        for (int y = 0; y < sideCount; y++)
        {
            for (int x = 0; x < sideCount; x++)
            {
                Vector3 point = transform.position + new Vector3(0 - (walkMonsterSize / 2) + x * spacing, 0, 0 - (walkMonsterSize / 2) + y * spacing);
                raycasts[y * sideCount + x] = new RaycastCommand(point, Vector3.down, 255);
            }
        }

        raycastAllCommand = new RaycastAllCommand(raycasts, hits, maxHits);
        raycastAllCommandJob = raycastAllCommand.Schedule(default(JobHandle));

        // Wait for the batch processing job to complete
        //handle.Complete();
        

        freePoints = new NativeQueue<PointStruct>(Allocator.TempJob);
        blockedPoints = new NativeQueue<PointStruct>(Allocator.TempJob);

        pollsJob = new GeneratePolls()
        {
            freePoints = freePoints.AsParallelWriter(),
            blockedPoints = blockedPoints.AsParallelWriter(),
            hits = hits,
            walkMonsterSize = walkMonsterSize,
            sideCount = sideCount,
            playerHeight = playerHeight,
            stepHeight = stepHeight,
            playerRadius = playerRadius
        };

        pollsJobHandle = pollsJob.Schedule(sideCount * sideCount, 64, raycastAllCommandJob);
    }

    private void LateUpdate()
    {
        pollsJobHandle.Complete();


        //raycastAllCommandJob.Complete();

        raycastAllCommand.Dispose();
        raycasts.Dispose();
        freePoints.Dispose();
        blockedPoints.Dispose();
    }


    Vector3 offset = new Vector3(0, 0.01f, 0);


    void OnRenderObject()
    {
        GL.PushMatrix();

        if (lineMaterial == null)
            lineMaterial = GL_Utils.CreateLineMaterial();

        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        DisplayGrid();


        float spacing = (1f / (sideCount - 1)) * walkMonsterSize;

        if (hits != null && hits.Length > 0)
        {
            for (int x = 0; x < sideCount * sideCount; x++)
            {
                for (int i = 0; i < maxHits; i++)
                {
                    if (hits[x * maxHits + i].collider != null)
                    {
                        GL_Utils.DrawCrossVertical(hits[x * maxHits + i].point + offset, Color.white);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    public void DisplayGrid()
    {


        //if (DisplayVaildPoints || DisplayBlockedPoints)
        //{
        //    foreach (var p in polls)
        //    {
        //        Vector3 point = p.postition;
        //        if (DisplayVaildPoints)
        //            foreach (var f in p.yHeights)
        //            {
        //                point.y = f;
        //                GL_Utils.DrawCrossVertical(point, Color.white);
        //            }

        //        if (DisplayBlockedPoints)
        //            foreach (var f in p.yBlockedHeights)
        //            {
        //                point.y = f;
        //                GL_Utils.DrawCrossVertical(point, Color.red);
        //            }
        //    }
        //}

        //if (DisplayWalkPoints || DisplayFreeWalkPointEdges || DisplayBlockedWalkPointEdges)
        //{
        //    foreach (WalkCellStruct w in walkCells)
        //    {
        //        foreach (WalkPointGroupStruct walkPoint in w.walkGroups)
        //        {
        //            //if (walkPoint.points.Length == 0)
        //            //    continue;
        //            //if (DisplayWalkPoints)
        //            //    for (int i = 0; i < walkPoint.points.Length - 1; i++)
        //            //    {
        //            //        GL_Utils.DrawLine(walkPoint.points[i] + offset, walkPoint.points[i + 1] + offset, Color.white);
        //            //    }

        //            //if (walkPoint.walkEdgePoints != null)
        //            //{
        //            //    if (DisplayFreeWalkPointEdges && walkPoint.walkPointConfiguration > 0 || DisplayBlockedWalkPointEdges && walkPoint.walkPointConfiguration < 0)
        //            //        for (int i = 0; i < walkPoint.walkEdgePoints.Length - 1; i++)
        //            //        {
        //            //            Color walkEdgeColor = (walkPoint.walkPointConfiguration > 0) ? Color.green : Color.red;

        //            //            GL_Utils.DrawLine(walkPoint.walkEdgePoints[i] + offset, walkPoint.walkEdgePoints[i + 1] + offset, walkEdgeColor);
        //            //        }
        //            //}
        //        }
        //    }
        //}
    }


    private struct GeneratePolls : IJobParallelFor
    {
        public NativeArray<RaycastHit> hits;
        [WriteOnly]
        public NativeQueue<PointStruct>.ParallelWriter freePoints;
        [WriteOnly]
        public NativeQueue<PointStruct>.ParallelWriter blockedPoints;

        public float walkMonsterSize;
        public int sideCount;
        public float playerHeight;
        public float stepHeight;
        public float playerRadius;

        public Vector3 pos;

        public void Execute(int index)
        {
            pos.y = maxHeight;

            int y = index / sideCount;
            int x = index % sideCount;

            float spacing = (1f / (sideCount - 1)) * walkMonsterSize;

            
            Vector3 point = pos + new Vector3(0 - (walkMonsterSize / 2) + x * spacing, 0, 0 - (walkMonsterSize / 2) + y * spacing);

            //Calculate index
            hits = MathUtils.RaycastHitSortByY(ref hits, index * maxHits, maxHits);

            List<float> vPoints = new List<float>();
            List<float> bPoints = new List<float>();

            FindValidPoints(ref vPoints, ref bPoints, hits, index * maxHits, maxHits);
            RemoveCloseValues(ref vPoints, ref bPoints, stepHeight * 0.9f);

            foreach (var p in vPoints)
            {
                freePoints.Enqueue(new PointStruct(point + Vector3.up * p, index));
            }

            foreach (var p in bPoints)
            {
                blockedPoints.Enqueue(new PointStruct(point + Vector3.up * p, index));
            } 
        }


        private void FindValidPoints(ref List<float> validPoints, ref List<float> blockedPoints, NativeArray<RaycastHit> hits, int startIndex, int maxHits)
        {
            for (int i = startIndex; i < startIndex + maxHits && hits[i].point != default; i++)
            {
                RaycastHit hit = hits[i];

                if (i != startIndex + maxHits - 1 && hits[i + 1].point != default && hit.point.y + playerHeight > hits[i + 1].point.y)
                {
                    blockedPoints.Add(hit.point.y);
                    continue;
                }

                validPoints.Add(hit.point.y);
            }
        }

        private void RemoveCloseValues(ref List<float> validPoints, ref List<float> blockedPoints, float v)
        {
            //throw new NotImplementedException();
        }
    }

    //private struct GenerateWalkCells : IJobParallelFor
    //{
    //    public NativeArray<PointStruct> polls;
    //    public NativeArray<WalkCellStruct> walkCells;

    //    public float walkMonsterSize;
    //    public int sideCount;
    //    public float playerHeight;
    //    public float stepHeight;
    //    public float playerRadius;


    //    public void Execute(int index)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}
