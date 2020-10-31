using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WalkGrid
{
    public bool drawRay = false;
    public bool drawCross = false;

    public int maxHeight = 255;

    public float stepHeight = 0.9f;

    public CellGrid cellGrid;

    Color[] colors = { Color.cyan, Color.magenta, Color.green };


    public void GenerateWalkGrid(float squareSize, int sideCount, float playerHeight, float playerRadius, Vector3 pos)
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

        cellGrid = new CellGrid(polls, spacing, playerRadius);
    }

    public void DisplayGrid()
    {
        Vector3 offset = new Vector3(0, 0.01f, 0);


        foreach (WalkCell w in cellGrid.walkCells)
        {
            foreach (WalkCell.WalkPointGroup walkPoint in w.walkGroups)
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


    public class CellGrid
    {
        public WalkCell[,] walkCells;

        public CellGrid(Poll[,] map, float pollSpacing, float playerRadius)
        {
            int gridLength = map.GetLength(0);


            walkCells = new WalkCell[gridLength - 1, gridLength - 1];

            for (int x = 0; x < gridLength - 1; x++)
            {
                for (int y = 0; y < gridLength - 1; y++)
                {
                    walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y], pollSpacing);
                }
            }

            bool[,] culledTestPoints;

            Erode(ref map, out culledTestPoints, pollSpacing, playerRadius, gridLength);

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
                        walkCells[x, y] = new WalkCell(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y], pollSpacing);
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

        private void Erode(ref Poll[,] map, out bool[,] culledTestPoints, float pollSpacing, float playerRadius, int gridLength)
        {
            culledTestPoints = new bool[gridLength, gridLength];

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
                                                //Debug.Log("Removing");
                                                map[i, j].yHeights.Remove(p);
                                            }
                                        }
                                    }
                                }
                            }
                            //GL_Utils.DrawCircle(wE, playerRadius, Vector3.up, Color.red);
                        }
                    }
                }
            }

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



    public class WalkCell
    {
        private const float stepHeight = 1;
        public List<WalkPointGroup> walkGroups = new List<WalkPointGroup>();

        public WalkCell(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft, float pollSpacing)
        {

            FillWalkPoints(topLeft, topRight, bottomRight, bottomLeft, pollSpacing);
        }

        void FillWalkPoints(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft, float pollSpacing)
        {
            List<Vector3> points = new List<Vector3>(8);

            List<float> tLeft = new List<float>(topLeft.yHeights);
            List<float> tRight = new List<float>(topRight.yHeights);
            List<float> bRight = new List<float>(bottomRight.yHeights);
            List<float> bLeft = new List<float>(bottomLeft.yHeights);

            short configuration = 0;

            foreach (float p in tLeft)
            {
                points.Clear();
                configuration = 0;


                points.Add(topLeft.postition + Vector3.up * p);
                configuration += 8;// 8 = top left


                foreach (float p1 in tRight)
                {
                    if (Mathf.Abs(p - p1) < stepHeight)
                    {
                        points.Add(topRight.postition + Vector3.up * p1);
                        configuration += 4;// 4 = top right
                        tRight.Remove(p1);
                        break;
                    }
                }

                foreach (float p2 in bRight)
                {
                    if (Mathf.Abs(p - p2) < stepHeight)
                    {
                        points.Add(bottomRight.postition + Vector3.up * p2);
                        configuration += 2;// 2 = bottom right
                        bRight.Remove(p2);
                        break;
                    }
                }

                foreach (float p3 in bLeft)
                {
                    if (Mathf.Abs(p - p3) < stepHeight)
                    {
                        points.Add(bottomLeft.postition + Vector3.up * p3);
                        configuration += 1;// 1 = bottom left
                        bLeft.Remove(p3);
                        break;
                    }
                }

                if (points.Count > 1)
                {
                    points.Add(topLeft.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing));
            }

            foreach (float p in tRight)
            {
                points.Clear();
                configuration = 0;

                points.Add(topRight.postition + Vector3.up * p);
                configuration += 4;// 4 = top right

                foreach (float p1 in bRight)
                {
                    if (Mathf.Abs(p - p1) < stepHeight)
                    {
                        points.Add(bottomRight.postition + Vector3.up * p1);
                        configuration += 2;// 2 = bottom right
                        bRight.Remove(p1);
                        break;
                    }
                }

                foreach (float p2 in bLeft)
                {
                    if (Mathf.Abs(p - p2) < stepHeight)
                    {
                        points.Add(bottomLeft.postition + Vector3.up * p2);
                        configuration += 1;// 1 = bottom left
                        bLeft.Remove(p2);
                        break;
                    }
                }

                if (points.Count > 1)
                {
                    points.Add(topRight.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing));
            }

            foreach (float p in bRight)
            {
                points.Clear();
                configuration = 0;

                points.Add(bottomRight.postition + Vector3.up * p);
                configuration += 2;// 2 = bottom right

                foreach (float p1 in bLeft)
                {
                    if (Mathf.Abs(p - p1) < stepHeight)
                    {
                        points.Add(bottomLeft.postition + Vector3.up * p1);
                        configuration += 1;// 1 = bottom left
                        bLeft.Remove(p1);
                        break;
                    }
                }

                if (points.Count > 1)
                {
                    points.Add(bottomRight.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing));
            }

            foreach (float p in bLeft)
            {
                points.Clear();
                configuration = 0;

                points.Add(bottomLeft.postition + Vector3.up * p);
                configuration += 1;// 1 = bottom left

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing));
            }
        }

        public class WalkPointGroup
        {
            public Vector3[] points;

            // 15 = all four points are insde the walkgroup
            // 8 = top left
            // 4 = top right
            // 2 = bottom right
            // 1 = bottom left
            public short walkPointConfiguration;

            public Vector3[] walkEdgePoints;

            public WalkPointGroup(Vector3[] points, short walkPointConfiguration, float pollSpacing)
            {
                this.points = points;
                this.walkPointConfiguration = walkPointConfiguration;

                walkEdgePoints = CreateWalkEdge(pollSpacing);
            }


            Vector3[] CreateWalkEdge(float pollSpacing)
            {
                Vector3[] ret = null;


                switch (walkPointConfiguration)
                {
                    case 0: //You should not be able to have a WalkPointGroup with zero points
                        Debug.LogError("WalkPointGroup has a walkPointConfiguration that out of bounds: " + walkPointConfiguration);
                        break;

                    // 1 point :
                    case 1:
                    case 2:
                    case 4:
                    case 8:
                        ret = FindWalkEdge1Point(walkPointConfiguration, pollSpacing);
                        break;

                    // 2 point :
                    case 3:
                    case 5:
                    case 6:
                    case 9:
                    case 10:
                    case 12:
                        ret = FindWalkEdge2Point(walkPointConfiguration, pollSpacing);
                        break;

                    // 3 point :
                    case 7:
                    case 11:
                    case 13:
                    case 14:
                        ret = FindWalkEdge3Point(walkPointConfiguration, pollSpacing);
                        break;


                    // 4 point :
                    case 15:
                        break;

                    default:
                        Debug.LogError("WalkPointGroup has a walkPointConfiguration that out of bounds: " + walkPointConfiguration);
                        break;
                }

                return ret;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Vector3[] FindWalkEdge1Point(int confg, float pollSpacing)
            {
                int verticalModifier = 0, horizontalModifier = 0;

                switch (confg)
                {
                    case  1: //Bottom Left
                        verticalModifier = 1;
                        horizontalModifier = 1;
                        break;

                    case 2: //Bottom Right
                        verticalModifier = 1;
                        horizontalModifier = -1;
                        break;

                    case 4: //Top Right
                        verticalModifier = -1;
                        horizontalModifier = -1;
                        break;

                    case 8: //Top Left
                        verticalModifier = -1;
                        horizontalModifier = 1;
                        break;
                    default:
                        Debug.LogError("WalkPointGroup with one point has a walkPointConfiguration that out of bounds: " + confg);
                        break;
                }

                Vector3[] ret = null;

                if (verticalModifier != 0 && horizontalModifier != 0)
                {
                    ret = new Vector3[3];

                    ret[0] = points[0];
                    ret[1] = points[0];
                    ret[2] = points[0];

                    SetWalkEdgePointsFor1Point(ref ret, verticalModifier, horizontalModifier, pollSpacing, stepHeight, 2);
                }

                return ret;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            //Helper function for setting WalkEdgePoints based off of only one point
            void SetWalkEdgePointsFor1Point(ref Vector3[] ret, int verticalModifier, int horizontalModifier, float pollSpacing,
                                            float playerStepHeight, float playerHeight, int loopCount = 3)
            {
                RaycastHit[] hits = new RaycastHit[255]; 
                Vector3 rayHeight = new Vector3(0, 255, 0);
                float rayLength = rayHeight.y + playerStepHeight;
                int hitCount;


                pollSpacing /= 2;

                ret[0].z += pollSpacing * verticalModifier;
                ret[1].z += pollSpacing * verticalModifier;
                ret[1].x += pollSpacing * horizontalModifier;
                ret[2].x += pollSpacing * horizontalModifier;


                float pointHeight = ret[0].y;

                for (int i = 0; i < loopCount; i++)
                {
                    pollSpacing /= 2;

                    //Vertical
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[0] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[0], hits, hitCount, playerHeight, pointHeight, pointHeight))
                        {
                            ret[0].z += pollSpacing * verticalModifier;
                        }
                        else
                        {
                            ret[0].z -= pollSpacing * verticalModifier;
                        }
                    }
                    else
                    {
                        ret[0].z -= pollSpacing * verticalModifier;
                    }

                    //45 degre
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[1] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[1], hits, hitCount, playerHeight, pointHeight, pointHeight))
                        {
                            ret[1].z += pollSpacing * verticalModifier;
                            ret[1].x += pollSpacing * horizontalModifier;
                        }
                        else
                        {
                            ret[1].z -= pollSpacing * verticalModifier;
                            ret[1].x -= pollSpacing * horizontalModifier;
                        }
                    }
                    else
                    {
                        ret[1].z -= pollSpacing * verticalModifier;
                        ret[1].x -= pollSpacing * horizontalModifier;
                    }

                    //Horizontal
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[2] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[2], hits, hitCount, playerHeight, pointHeight, pointHeight))
                        {
                            ret[2].x += pollSpacing * horizontalModifier;
                        }
                        else
                        {
                            ret[2].x -= pollSpacing * horizontalModifier;
                        }
                    }
                    else
                    {
                        ret[2].x -= pollSpacing * horizontalModifier;
                    }
                }
            }

            

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Vector3[] FindWalkEdge2Point(int confg, float pollSpacing)
            {
                Vector3[] ret = new Vector3[2];

                int p0H = 0, p0V = 0, p1H = 0, p1V = 0;

                ret[0] = points[0];
                ret[1] = points[1];

                switch (confg)
                {
                    case 3: //Bottom Left and Bottom Right
                        p0V = 1;
                        p1V = 1;
                        break;
                    case 5: // Bottom Left and Top Right
                        //This case is very unlikely
                        //So just add points[0] and points[1] to the list of walkedgepoints
                        //We add them to the list of walkEdges so that they still contribute to the eroding process
                        break;
                    case 6: // Bottom Right and Top Right
                        p0H = -1;
                        p1H = -1;
                        break;
                    case 9: // Bottom Left and Top Left
                        p0H = 1;
                        p1H = 1;
                        break;
                    case 10: //Top Left and Bottom Right
                        //This case is very unlikely
                        //So just add points[0] and points[1] to the list of walkedgepoints
                        //We add them to the list of walkEdges so that they still contribute to the eroding process
                        break;
                    case 12: //Top Left and Top Right
                        p0V = -1;
                        p1V = -1;
                        break;
                    default:
                        Debug.LogError("WalkPointGroup with one point has a walkPointConfiguration that out of bounds: " + confg);
                        ret = null;
                        break;
                }


                if (p0H != 0 || p0V != 0 || p1H != 0 || p1V != 0)
                {
                    SetWalkEdgePointsFor2Point(ref ret, p0H, p0V, p1H, p1V, pollSpacing, stepHeight, 2);
                }

                return ret;
            }

            //Helper function for setting WalkEdgePoints based off of two points
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void SetWalkEdgePointsFor2Point(ref Vector3[] ret, int p0H, int p0V, int p1H, int p1V, float pollSpacing, float playerStepHeight, float playerHeight, int loopCount = 3)
            {
                RaycastHit[] hits = new RaycastHit[255];
                Vector3 rayHeight = new Vector3(0, 255, 0);
                float rayLength = rayHeight.y + playerStepHeight;
                int hitCount;

                float minHeight = Mathf.Min(ret[0].y, ret[1].y);
                float maxHeight = Mathf.Max(ret[0].y, ret[1].y);

                pollSpacing /= 2;

                ret[0].x += pollSpacing * p0H;
                ret[0].z += pollSpacing * p0V;

                ret[1].x += pollSpacing * p1H;
                ret[1].z += pollSpacing * p1V;

                for (int i = 0; i < loopCount; i++)
                {
                    pollSpacing /= 2;
                    //p0
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[0] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[0], hits, hitCount, playerHeight, minHeight, maxHeight))
                        {
                            ret[0].x += pollSpacing * p0H;
                            ret[0].z += pollSpacing * p0V;
                        }
                        else
                        {
                            ret[0].x -= pollSpacing * p0H;
                            ret[0].z -= pollSpacing * p0V;
                        }
                    }
                    else
                    {
                        ret[0].x -= pollSpacing * p0H;
                        ret[0].z -= pollSpacing * p0V;
                    }

                    //p1
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[1] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[1], hits, hitCount, playerHeight, minHeight, maxHeight))
                        {
                            ret[1].x += pollSpacing * p1H;
                            ret[1].z += pollSpacing * p1V;
                        }
                        else
                        {
                            ret[1].x -= pollSpacing * p1H;
                            ret[1].z -= pollSpacing * p1V;
                        }
                        
                    }
                    else
                    {
                        ret[1].x -= pollSpacing * p1H;
                        ret[1].z -= pollSpacing * p1V;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Vector3[] FindWalkEdge3Point(int confg, float pollSpacing)
            {
                Vector3[] ret = new Vector3[3]; ;

                int p0H = 0, p0V = 0, p1H = 0, p1V = 0, p2H = 0, p2V = 0;

                ret[0] = points[0];
                ret[1] = points[1];
                ret[2] = points[2];

                switch (confg)
                {
                    case 7:  // Top Left Missing
                        p0H = -1;
                        p1H = -1;
                        p1V = 1;
                        p2V = 1;
                        break;
                    case 11: // Top Right Missing

                        ret[0] = points[0];
                        ret[1] = points[2];
                        ret[2] = points[1];

                        p0H = 1;
                        p1H = 1;
                        p1V = 1;
                        p2V = 1;
                        break;
                    case 13: // Bottom Right Missing

                        ret[0] = points[1];
                        ret[1] = points[0];
                        ret[2] = points[2];

                        p0V = -1;
                        p1H = 1;
                        p1V = -1;
                        p2H = 1;
                        break;
                    case 14: // Bottom Left Missing
                        p0V = -1;
                        p1H = -1;
                        p1V = -1;
                        p2H = -1;
                        break;
                    default:
                        Debug.LogError("WalkPointGroup with one point has a walkPointConfiguration that out of bounds: " + confg);
                        ret = null;
                        break;
                }


                if (p0H != 0 || p0V != 0 || p1H != 0 || p1V != 0 || p2H != 0 || p2V != 0)
                {
                    SetWalkEdgePointsFor3Point(ref ret, p0H, p0V, p1H, p1V, p2H, p2V, pollSpacing, stepHeight, 2);
                }

                return ret;
            }

            //Helper function for setting WalkEdgePoints based off of three points
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void SetWalkEdgePointsFor3Point(ref Vector3[] ret, int p0H, int p0V, int p1H, int p1V, int p2H, int p2V, float pollSpacing, float playerStepHeight, float playerHeight, int loopCount = 3)
            {
                RaycastHit[] hits = new RaycastHit[255];
                Vector3 rayHeight = new Vector3(0, 255, 0);
                float rayLength = rayHeight.y + playerStepHeight;
                int hitCount;

                float minHeight = Mathf.Min(ret[0].y, ret[1].y);
                float maxHeight = Mathf.Max(ret[0].y, ret[1].y);

                pollSpacing /= 2;

                ret[0].x += pollSpacing * p0H;
                ret[0].z += pollSpacing * p0V;

                ret[1].x += pollSpacing * p1H;
                ret[1].z += pollSpacing * p1V;

                ret[2].x += pollSpacing * p2H;
                ret[2].z += pollSpacing * p2V;

                for (int i = 0; i < loopCount; i++)
                {
                    pollSpacing /= 2;
                    //p0
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[0] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[0], hits, hitCount, playerHeight, minHeight, maxHeight))
                        {
                            ret[0].x += pollSpacing * p0H;
                            ret[0].z += pollSpacing * p0V;
                        }
                        else
                        {
                            ret[0].x -= pollSpacing * p0H;
                            ret[0].z -= pollSpacing * p0V;
                        }
                    }
                    else
                    {
                        ret[0].x -= pollSpacing * p0H;
                        ret[0].z -= pollSpacing * p0V;
                    }

                    //p1
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[1] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[1], hits, hitCount, playerHeight, minHeight, maxHeight))
                        {
                            ret[1].x += pollSpacing * p1H;
                            ret[1].z += pollSpacing * p1V;
                        }
                        else
                        {
                            ret[1].x -= pollSpacing * p1H;
                            ret[1].z -= pollSpacing * p1V;
                        }
                    }
                    else
                    {
                        ret[1].x -= pollSpacing * p1H;
                        ret[1].z -= pollSpacing * p1V;
                    }

                    //p2
                    if (0 < (hitCount = Physics.RaycastNonAlloc(ret[2] + rayHeight, Vector3.down, hits, rayLength)))
                    {
                        hits = MathUtils.RaycastHitSortByY(hits, hitCount);

                        if (FindValidPoint(ref ret[2], hits, hitCount, playerHeight, minHeight, maxHeight))
                        {
                            ret[2].x += pollSpacing * p2H;
                            ret[2].z += pollSpacing * p2V;
                        }
                        else
                        {
                            ret[2].x -= pollSpacing * p2H;
                            ret[2].z -= pollSpacing * p2V;
                        }
                        
                    }
                    else
                    {
                        ret[2].x -= pollSpacing * p2H;
                        ret[2].z -= pollSpacing * p2V;
                    }
                }
            }

            private static bool FindValidPoint(ref Vector3 ret, RaycastHit[] hits, int hitCount, float playerHeight, float minHeight, float maxHeight)
            {
                List<Collider> blockerCollider = new List<Collider>();

                for (int h = 0; h < hitCount; h++)
                {
                    bool valid = true;

                    RaycastHit hit = hits[h];

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

                    if (valid)
                    {
                        if (hit.point.y < minHeight - stepHeight || hit.point.y > maxHeight + stepHeight)
                        {
                            valid = false;
                        }

                        if (h != hitCount - 1 && (hit.point.y + playerHeight > hits[h + 1].point.y || Physics.Raycast(hit.point, Vector3.up, playerHeight)))
                        {
                            valid = false;
                        }
                    }

                    if (valid)
                    {
                        ret.y = hit.point.y;
                        return true;
                    }
                }

                return false;
            }
        }
    }

    public class Poll
    {
        public Vector3 postition;
        public List<float> yHeights; //0 is the top most Y value while n is the lowest Y value
        public short pointCount;

        public Poll(Vector3 _pos, List<float> _yheights)
        {
            postition = _pos;
            yHeights = _yheights;
            pointCount = (short)yHeights.Count;
        }
    }


    private static bool IsPointInCollider(Collider c, Vector3 point)
    {
        Vector3 closest = c.ClosestPoint(point);
        // Because closest=point if point is inside - not clear from docs I feel
        return closest == point;
    }
}