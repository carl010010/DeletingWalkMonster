using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MathUtils;

public class CellGrid
{
    public WalkCell[,] walkCells;

    public CellGrid(Poll[,] map, float pollSpacing, float playerHeight, float stepHeight, float playerRadius)
    {
        int gridLength = map.GetLength(0);


        walkCells = new WalkCell[gridLength - 1, gridLength - 1];

        for (int x = 0; x < gridLength - 1; x++)
        {
            for (int y = 0; y < gridLength - 1; y++)
            {
                walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y], pollSpacing, playerHeight, stepHeight);
            }
        }

        bool[,] culledTestPoints;
        List<Cylinder> cylinders;

        Erode(ref map, out culledTestPoints, out cylinders, pollSpacing, gridLength, playerRadius, 1);

        for (int x = 0; x < gridLength - 1; x++)
        {
            for (int y = 0; y < gridLength - 1; y++)
            {
                //TODO Is this optimization helpfull?
                if (culledTestPoints[x, y] == true ||
                    culledTestPoints[x + 1, y] == true ||
                    culledTestPoints[x, y + 1] == true ||
                    culledTestPoints[x + 1, y + 1] == true)
                {
                    walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y], pollSpacing, playerHeight, stepHeight, cylinders);
                }
            }
        }

        //TODO Try doing the extra ones after instead of while inside the main for loop
        //if (culledTestPoints[x, y] == true ||
        //            culledTestPoints[x + 1, y] == true ||
        //            culledTestPoints[x, y + 1] == true ||
        //            culledTestPoints[x + 1, y + 1] == true)
        //{
        //    walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y], pollSpacing);
        //}
    }

    private void Erode(ref Poll[,] map, out bool[,] culledTestPoints, out List<Cylinder> cylinders, float pollSpacing, int gridLength, float playerRadius, float playerStepHeight)
    {
        culledTestPoints = new bool[gridLength, gridLength];
        cylinders = new List<Cylinder>();

        //errod
        // foreach walkEdgePoints erods poll map
        for (int x = 0; x < gridLength - 1; x++)
        {
            for (int y = 0; y < gridLength - 1; y++)
            {
                foreach (var wG in walkCells[x, y].walkGroups)
                {
                    if (wG == null || wG.walkEdgePoints == null)
                        continue;

                    foreach (var wE in wG.walkEdgePoints)
                    {
                        int count = Mathf.CeilToInt(playerRadius * 2 / pollSpacing);
                        int offset = Mathf.CeilToInt(count / 2f);

                        int i = Mathf.Max(0, x - offset);
                        for (; i < gridLength && i < x + offset + 1; i++)
                        {
                            int j = Mathf.Max(0, y - offset);
                            for (; j < gridLength && j < y + offset + 1; j++)
                            {
                                Vector2 mapPos = new Vector2(map[i, j].postition.x, map[i, j].postition.z);
                                Vector2 walkPos = new Vector2(wE.x, wE.z);

                                if (map[i, j].pointCount != 0 && (mapPos - walkPos).sqrMagnitude < playerRadius * playerRadius)
                                {
                                    culledTestPoints[i, j] = true;
                                    for (int a = map[i, j].yHeights.Count - 1; a >= 0; a--)
                                    {
                                        float p = map[i, j].yHeights[a];

                                        if (Mathf.Abs(p - wE.y) < 0.5)
                                        {
                                            map[i, j].yHeights.Remove(p);
                                        }
                                    }
                                }
                            }
                        }
                        cylinders.Add(new Cylinder(wE - Vector3.up * playerStepHeight, playerStepHeight, playerRadius));
                        GL_Utils.DrawCircle(wE, playerRadius, Vector3.up, Color.red);
                    }
                }
            }
        }

        //We only need distinct cylinders
        cylinders = cylinders.GroupBy(c => c.pos).Select(c => c.First()).ToList();


        //for (int x = 0; x < gridLength; x++)
        //{
        //    for (int y = 0; y < gridLength; y++)
        //    {
        //        if (culledTestPoints[x, y] == true)
        //        {
        //            GL_Utils.DrawCrossVertical(map[x, y].postition, Color.cyan);
        //        }

        //    }
        //}
    }
}
