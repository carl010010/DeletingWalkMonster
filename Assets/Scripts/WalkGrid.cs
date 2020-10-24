using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class WalkGrid
{
    public bool drawRay = false;
    public bool drawCross = false;

    public CellGrid cellGrid;

    public void GenerateWalkGrid(float squareSize, int sideCount, float playerHeight, Vector3 pos)
    {
        if (squareSize <= 0.01 || sideCount < 1)
            return;


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

                        foreach (var c in blockerCollider)
                        {
                            if (IsPointInCollider(c, hit.point))
                            {
                                valid = false;
                                break;
                            }
                        }


                        if (hit.collider.gameObject.layer == 8)
                        {
                            blockerCollider.Add(hit.collider);
                        }
                        
                        if (valid && (h != 0
                            && hit.point.y + playerHeight > hits[h - 1].point.y
                            || Physics.Raycast(hit.point, Vector3.up, out hit2, playerHeight)))
                                //TODO (Carl) can significantly reduce the amount of raycasts
                                // if we assume hits[0] is the top most point
                                // in the world
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
                polls[x, y] = new Poll(new Vector3(point.x, 0, point.z), validPoints.ToArray());
            }
        }

        cellGrid = new CellGrid(polls);
    }

    public void DisplayGrid()
    {
        Vector3 offset = new Vector3(0, 0.01f, 0);


        foreach(WalkCell w in cellGrid.walkCells)
        {
            foreach (var walkPoint in w.walkPoints)
            {
                if (walkPoint.points.Length == 0)
                    continue;

                if (walkPoint.points.Length == 1)
                    GL_Utils.DrawCrossVertical(walkPoint.points[0] + offset, Color.white);
                else
                {
                    for (int i = 0; i < walkPoint.points.Length - 1; i++)
                    {
                        GL_Utils.DrawLine(walkPoint.points[i] + offset, walkPoint.points[i + 1] + offset, Color.white);
                    }
                }
            }
        }
    }


    public class CellGrid
    {
        public WalkCell[,] walkCells;

        public CellGrid(Poll[,] map)
        {
            int gridLength = map.GetLength(0);


            walkCells = new WalkCell[gridLength - 1, gridLength - 1];

            for(int x = 0; x < gridLength - 1; x++ )
            {
                for (int y = 0; y < gridLength - 1; y++)
                {
                    walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y]);
                }
            }
        }
    }



    public class WalkCell
    {
        public List<WalkPointGroup> walkPoints = new List<WalkPointGroup>();

        public WalkCell(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft)
        {

            FillWalkPoints(topLeft, topRight, bottomRight, bottomLeft);


        }

        void FillWalkPoints(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft)
        {
            List<Vector3> points = new List<Vector3>(8);

            for (int a = 0; a < topLeft.pointCount; a++)
            {
                for (int b = 0; b < topRight.pointCount; b++)
                {
                    for (int c = 0; c < bottomRight.pointCount; c++)
                    {
                        for (int d = 0; d < bottomLeft.pointCount; d++)
                        {
                            points.Clear();

                            points.Add(bottomLeft.postition + Vector3.up * bottomLeft.yHeights[d]);

                            if (a < topLeft.yHeights.Length && Mathf.Abs(bottomLeft.yHeights[d] - topLeft.yHeights[a]) < 0.9f)
                            {
                                points.Add(topLeft.postition + Vector3.up * topLeft.yHeights[a]);
                                    a++;
                            }

                            if (b < topRight.yHeights.Length && Mathf.Abs(bottomLeft.yHeights[d] - topRight.yHeights[b]) < 0.9f)
                            {
                                points.Add(topRight.postition + Vector3.up * topRight.yHeights[b]);
                                b++;
                            }

                            if (c < bottomRight.yHeights.Length && Mathf.Abs(bottomLeft.yHeights[d] - bottomRight.yHeights[c]) < 0.9f)
                            {
                                points.Add(bottomRight.postition + Vector3.up * bottomRight.yHeights[c]);
                                c++;
                            }

                            if(points.Count > 1)
                            {
                                points.Add(bottomLeft.postition + Vector3.up * bottomLeft.yHeights[d]);
                            }

                            walkPoints.Add(new WalkPointGroup(points.ToArray()));
                        }

                        points.Clear();

                        if (c >= bottomRight.yHeights.Length)
                            break;


                        points.Add(bottomRight.postition + Vector3.up * bottomRight.yHeights[c]);


                        if (a < topLeft.yHeights.Length && Mathf.Abs(bottomRight.yHeights[c] - topLeft.yHeights[a]) < 0.9f)
                        {
                            points.Add(topLeft.postition + Vector3.up * topLeft.yHeights[a]);
                            a++;
                        }

                        if (b < topRight.yHeights.Length && Mathf.Abs(bottomRight.yHeights[c] - topRight.yHeights[b]) < 0.9f)
                        {
                            points.Add(topRight.postition + Vector3.up * topRight.yHeights[b]);
                            b++;
                        }


                        if (points.Count > 1)
                        {
                            points.Add(bottomRight.postition + Vector3.up * bottomRight.yHeights[c]);
                        }

                        walkPoints.Add(new WalkPointGroup(points.ToArray()));
                    }

                    points.Clear();

                    if (b >= topRight.yHeights.Length)
                        break;

                    points.Add(topRight.postition + Vector3.up * topRight.yHeights[b]);

                    if (a < topLeft.yHeights.Length && Mathf.Abs(topRight.yHeights[b] - topLeft.yHeights[a]) < 0.9f)
                    {
                        points.Add(topLeft.postition + Vector3.up * topLeft.yHeights[a]);
                        a++;
                    }

                    if (points.Count > 1)
                    {
                        points.Add(topRight.postition + Vector3.up * topRight.yHeights[b]);
                    }

                    walkPoints.Add(new WalkPointGroup(points.ToArray()));
                }
            }
        }

        public class WalkPointGroup
        {
            public Vector3[] points;
            //public Tuple<int, int>[] walkEdges;

            public WalkPointGroup(Vector3[] points)//, Tuple<int, int>[] walkEdges)
            {
                this.points = points;
                //this.walkEdges = walkEdges;
            }
        }
    }

    public class Poll
    {
        public Vector3 postition;
        public float[] yHeights; //0 is the top most Y value while n is the lowest Y value
        public short pointCount;

        public Poll(Vector3 _pos, float[] _yheights)
        {
            postition = _pos;
            yHeights = _yheights;
            pointCount = (short)yHeights.Length;
        }
    }


    private static bool IsPointInCollider(Collider c, Vector3 point)
    {
        Vector3 closest = c.ClosestPoint(point);
        // Because closest=point if point is inside - not clear from docs I feel
        return closest == point;
    }
}
