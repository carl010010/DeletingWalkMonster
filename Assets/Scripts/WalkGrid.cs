using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static MathUtils;

public class WalkGrid
{
    public bool drawRay = false;
    public bool drawCross = false;

    public int maxHeight = 255;

    public CellGrid cellGrid;

    Color[] colors = { Color.cyan, Color.magenta, Color.green };


    public void GenerateWalkGrid(float squareSize, int sideCount, float playerHeight, float stepHeight, float playerRadius, Vector3 pos)
    {
        if (squareSize <= 0.01 || sideCount < 1)
            return;

        pos.y = maxHeight;


        Poll[,] polls = new Poll[sideCount, sideCount];


        float spacing = (1f / (sideCount - 1)) * squareSize;

        for (int x = 0; x < sideCount; x++)
        {
            for (int y = 0; y < sideCount; y++)
            {
                Vector3 point = pos + new Vector3(0 - (squareSize / 2) + x * spacing, 0, 0 - (squareSize / 2) + y * spacing);

                RaycastHit[] hits = Physics.RaycastAll(point, Vector3.down);

                hits = hits.OrderBy(v => v.point.y).Reverse().ToArray();

                List<float> validPoints = new List<float>();

                if (hits.Length != 0)
                {

                    List<Collider> blockerCollider = new List<Collider>();

                    for (int h = 0; h < hits.Length; h++)
                    {
                        bool valid = true;

                        RaycastHit hit = hits[h];
                        RaycastHit hit2;

                        if (hit.collider.gameObject.layer == 8)
                        {
                            blockerCollider.Add(hit.collider);
                            valid = false;
                        }
                        else
                        {

                            foreach (var c in blockerCollider)
                            {
                                if (IsPointInCollider(c, hit.point))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        if (valid && h != 0
                            && (hit.point.y + playerHeight > hits[h - 1].point.y || Physics.Raycast(hit.point, Vector3.up, out hit2, playerHeight)))
                        {
                            valid = false;
                        }

                        if (valid)
                            validPoints.Add(hit.point.y);

                        if (drawCross)
                            GL_Utils.DrawCrossNormal(hit.point + Vector3.up * 0.01f, hit.normal, valid ? Color.white : Color.red);
                    }


                    if (drawRay)
                        GL_Utils.DrawLine(point, hits[hits.Length - 1].point, Color.green);
                }
                polls[x, y] = new Poll(new Vector3(point.x, 0, point.z), validPoints);
            }
        }

        cellGrid = new CellGrid(polls, spacing, playerHeight, stepHeight, playerRadius);
    }

    public void DisplayGrid()
    {
        Vector3 offset = new Vector3(0, 0.01f, 0);

        foreach (WalkCell w in cellGrid.walkCells)
        {
            foreach (WalkPointGroup walkPoint in w.walkGroups)
            {
                if (walkPoint.points.Length == 0)
                    continue;

                for (int i = 0; i < walkPoint.points.Length - 1; i++)
                {
                    GL_Utils.DrawLine(walkPoint.points[i] + offset, walkPoint.points[i + 1] + offset, Color.white);
                }

                if (walkPoint.walkEdgePoints != null)
                {
                    for (int i = 0; i < walkPoint.walkEdgePoints.Length - 1; i++)
                    {
                        GL_Utils.DrawLine(walkPoint.walkEdgePoints[i] + offset, walkPoint.walkEdgePoints[i + 1] + offset, Color.red);
                    }
                }
            }
        }
    }
}