using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace test
{

    [ExecuteAlways]
    public class FreeWalkEdgeTest : MonoBehaviour
    {
        public float SqrSize = 3;
        public int sideCount = 25;
        public float playerHeight = 2;
        public float stepHeight = 0.9f;
        public float playerRadius = 0.5f;

        Poll[,] polls;
        const int maxHeight = 255;
        CellGrid cellGrid;
        Material lineMaterial;


        [Header("Debug Display")]
        public bool DisplayVaildPoints = false;
        public bool DisplayBlockedPoints = false;
        public bool DisplayWalkPoints = false;
        public bool DisplayFreeWalkPointEdges = false;
        public bool DisplayBlockedWalkPointEdges = false;
        static public bool _DisplayCylinders = false;
        public bool DisplayCylinders = false;

        void Awake()
        {
            // must be called before trying to draw lines..
            lineMaterial = GL_Utils.CreateLineMaterial();
        }

        void OnRenderObject()
        {
            _DisplayCylinders = DisplayCylinders;


            GL.PushMatrix();

            if (lineMaterial == null)
                lineMaterial = GL_Utils.CreateLineMaterial();

            lineMaterial.SetPass(0);

            GL.Begin(GL.LINES);

            DisplayFreeWalkEdgesAndBlockedWalkEdges();
            DisplayGrid();

            GL.End();
            GL.PopMatrix();
        }


        void DisplayFreeWalkEdgesAndBlockedWalkEdges()
        {
            if (SqrSize <= 0.01 || sideCount < 1)
                return;

            if (polls == null || polls.GetLength(0) != sideCount)
            {
                polls = new Poll[sideCount, sideCount];
            }

            Vector3 pos = transform.position;
            pos.y = maxHeight;

            float spacing = (1f / (sideCount - 1)) * SqrSize;

            for (int x = 0; x < sideCount; x++)
            {
                for (int y = 0; y < sideCount; y++)
                {
                    Vector3 point = pos + new Vector3(0 - (SqrSize / 2) + x * spacing, 0, 0 - (SqrSize / 2) + y * spacing);

                    RaycastHit[] hits = Physics.RaycastAll(point, Vector3.down);
                    hits = MathUtils.RaycastHitSortByY(hits, hits.Length);

                    List<float> validPoints = new List<float>();
                    List<float> blockedPoints = new List<float>();

                    FindValidPoints(ref validPoints, ref blockedPoints, hits);
                    RemoveCloseValues(ref validPoints, ref blockedPoints, stepHeight * 0.9f);

                    //Y value must be set to zero because of the way Fill walk points works in WalkCell class
                    point.y = 0;
                    polls[x, y] = new Poll(point, validPoints, blockedPoints);

                    //foreach (var f in validPoints)
                    //{
                    //    point.y = f;
                    //    GL_Utils.DrawCrossVertical(point, Color.white);
                    //}

                    //foreach(var f in blockedPoints)
                    //{
                    //    point.y = f;
                    //    GL_Utils.DrawCrossVertical(point, Color.red);
                    //}
                }
            }


            cellGrid = new CellGrid(polls, spacing, playerHeight, stepHeight, playerRadius);

        }

        public void DisplayGrid()
        {
            Vector3 offset = new Vector3(0, 0.01f, 0);


            if (DisplayVaildPoints || DisplayBlockedPoints)
            {
                foreach (var p in polls)
                {
                    Vector3 point = p.postition;
                    if (DisplayVaildPoints)
                        foreach (var f in p.yHeights)
                        {
                            point.y = f;
                            GL_Utils.DrawCrossVertical(point, Color.white);
                        }

                    if (DisplayBlockedPoints)
                        foreach (var f in p.yBlockedHeights)
                        {
                            point.y = f;
                            GL_Utils.DrawCrossVertical(point, Color.red);
                        }
                }
            }

            if (DisplayWalkPoints || DisplayFreeWalkPointEdges || DisplayBlockedWalkPointEdges)
            {
                foreach (WalkCell w in cellGrid.walkCells)
                {
                    foreach (WalkPointGroup walkPoint in w.walkGroups)
                    {
                        if (walkPoint.points.Length == 0)
                            continue;
                        if (DisplayWalkPoints)
                            for (int i = 0; i < walkPoint.points.Length - 1; i++)
                            {
                                GL_Utils.DrawLine(walkPoint.points[i] + offset, walkPoint.points[i + 1] + offset, Color.white);
                            }

                        if (walkPoint.walkEdgePoints != null)
                        {
                            if (DisplayFreeWalkPointEdges && walkPoint.walkPointConfiguration > 0 || DisplayBlockedWalkPointEdges && walkPoint.walkPointConfiguration < 0)
                                for (int i = 0; i < walkPoint.walkEdgePoints.Length - 1; i++)
                                {
                                    Color walkEdgeColor = (walkPoint.walkPointConfiguration > 0) ? Color.green : Color.red;

                                    GL_Utils.DrawLine(walkPoint.walkEdgePoints[i] + offset, walkPoint.walkEdgePoints[i + 1] + offset, walkEdgeColor);
                                }
                        }
                    }
                }
            }
        }

        private void RemoveCloseValues(ref List<float> validPoints, ref List<float> blockedPoints, float v)
        {
            foreach (float f in validPoints)
            {
                for (int i = blockedPoints.Count - 1; i >= 0; i--)
                {
                    float bf = (float)blockedPoints[i];
                    if (Mathf.Abs(f - bf) < v)
                        blockedPoints.RemoveAt(i);
                }
            }
        }

        private void FindValidPoints(ref List<float> validPoints, ref List<float> blockedPoints, RaycastHit[] hits)
        {
            List<Collider> blockerCollider = new List<Collider>();
            int hitCount = hits.Length;

            for (int h = 0; h < hitCount; h++)
            {
                bool valid = true;
                RaycastHit hit = hits[h];

                if (hit.collider.gameObject.layer == 8)
                {
                    blockerCollider.Add(hit.collider);

                    blockedPoints.Add(hit.point.y);
                    continue;

                }

                foreach (Collider c in blockerCollider)
                {
                    if (MathUtils.IsPointInCollider(c, hit.point))
                    {
                        blockedPoints.Add(hit.point.y);
                        valid = false;
                        break;

                    }
                }

                if (!valid)
                    continue;


                if (h != hitCount - 1 && (hit.point.y + playerHeight > hits[h + 1].point.y || Physics.Raycast(hit.point, Vector3.up, playerHeight)))
                {
                    blockedPoints.Add(hit.point.y);
                    continue;
                }

                validPoints.Add(hit.point.y);
            }
        }
    }
}