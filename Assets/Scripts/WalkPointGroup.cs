using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace test
{
    public class WalkPointGroup
    {
        public Vector3[] points;

        float playerStepHeight;
        float playerHeight;

        // 15 = all four points are inside the walkgroup
        // 8 = top left
        // 4 = top right
        // 2 = bottom right
        // 1 = bottom left
        public short walkPointConfiguration;

        public Vector3[] walkEdgePoints;

        public WalkPointGroup(Vector3[] points, short walkPointConfiguration, float pollSpacing, float playerHeight, float playerStepHeight, List<MathUtils.Cylinder> cylinders)
        {
            this.points = points;
            this.walkPointConfiguration = walkPointConfiguration;
            this.playerHeight = playerHeight;
            this.playerStepHeight = playerStepHeight;

            walkEdgePoints = CreateWalkEdge(Mathf.Abs(walkPointConfiguration), pollSpacing, cylinders);
        }

        Vector3[] CreateWalkEdge(int config, float pollSpacing, List<MathUtils.Cylinder> cylinders)
        {
            Vector3[] ret = null;

            switch (config)
            {
                case 0: //You should not be able to have a WalkPointGroup with zero points
                    Debug.LogError("WalkPointGroup has a walkPointConfiguration that out of bounds: " + walkPointConfiguration);
                    break;

                // 1 point :
                case 1:
                case 2:
                case 4:
                case 8:
                    ret = FindWalkEdge1Point(config, pollSpacing, cylinders);
                    break;

                // 2 point :
                case 3:
                case 5:
                case 6:
                case 9:
                case 10:
                case 12:
                    ret = FindWalkEdge2Point(config, pollSpacing, cylinders);
                    break;

                // 3 point :
                case 7:
                case 11:
                case 13:
                case 14:
                    ret = FindWalkEdge3Point(config, pollSpacing, cylinders);
                    break;


                // 4 point :
                case 15:
                    break;

                default:
                    Debug.LogError("WalkPointGroup has a walkPointConfiguration that out of bounds: " + config);
                    break;
            }

            return ret;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3[] FindWalkEdge1Point(int confg, float pollSpacing, List<MathUtils.Cylinder> cylinders)
        {
            int verticalModifier = 0, horizontalModifier = 0;

            switch (confg)
            {
                case 1: //Bottom Left
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

                SetWalkEdgePointsFor1Point(ref ret, verticalModifier, horizontalModifier, pollSpacing, cylinders);
            }

            return ret;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //Helper function for setting WalkEdgePoints based off of only one point
        void SetWalkEdgePointsFor1Point(ref Vector3[] ret, int verticalModifier, int horizontalModifier, float pollSpacing, List<MathUtils.Cylinder> cylinders, int loopCount = 3)
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

                    if (FindValidPoint(ref ret[0], hits, hitCount, pointHeight, cylinders))
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

                    if (FindValidPoint(ref ret[1], hits, hitCount, pointHeight, cylinders))
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

                    if (FindValidPoint(ref ret[2], hits, hitCount, pointHeight, cylinders))
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3[] FindWalkEdge2Point(int confg, float pollSpacing, List<MathUtils.Cylinder> cylinders)
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
                SetWalkEdgePointsFor2Point(ref ret, p0H, p0V, p1H, p1V, pollSpacing, cylinders);
            }

            return ret;
        }

        //Helper function for setting WalkEdgePoints based off of two points
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetWalkEdgePointsFor2Point(ref Vector3[] ret, int p0H, int p0V, int p1H, int p1V, float pollSpacing, List<MathUtils.Cylinder> cylinders, int loopCount = 3)
        {
            RaycastHit[] hits = new RaycastHit[255];
            Vector3 rayHeight = new Vector3(0, 255, 0);
            float rayLength = rayHeight.y + playerStepHeight;
            int hitCount;

            float averageHeight = (ret[0].y + ret[1].y) / 2;

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

                    if (FindValidPoint(ref ret[0], hits, hitCount, averageHeight, cylinders))
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

                    if (FindValidPoint(ref ret[1], hits, hitCount, averageHeight, cylinders))
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

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3[] FindWalkEdge3Point(int confg, float pollSpacing, List<MathUtils.Cylinder> cylinders)
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
                SetWalkEdgePointsFor3Point(ref ret, p0H, p0V, p1H, p1V, p2H, p2V, pollSpacing, cylinders);
            }

            return ret;
        }

        //Helper function for setting WalkEdgePoints based off of three points
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetWalkEdgePointsFor3Point(ref Vector3[] ret, int p0H, int p0V, int p1H, int p1V, int p2H, int p2V, float pollSpacing, List<MathUtils.Cylinder> cylinders, int loopCount = 3)
        {
            RaycastHit[] hits = new RaycastHit[255];
            Vector3 rayHeight = new Vector3(0, 255, 0);
            float rayLength = rayHeight.y + playerStepHeight;
            int hitCount;

            float averageHeight = (ret[0].y + ret[1].y + ret[2].y) / 3;

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

                    if (FindValidPoint(ref ret[0], hits, hitCount, averageHeight, cylinders))
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

                    if (FindValidPoint(ref ret[1], hits, hitCount, averageHeight, cylinders))
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

                    if (FindValidPoint(ref ret[2], hits, hitCount, averageHeight, cylinders))
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

        private bool FindValidPoint(ref Vector3 ret, RaycastHit[] hits, int hitCount, float averageHeight, List<MathUtils.Cylinder> cylinders)
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
                    foreach (Collider c in blockerCollider)
                    {
                        if (MathUtils.IsPointInCollider(c, hit.point))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (valid)
                {
                    double minHeight = averageHeight - (playerStepHeight * walkPointConfiguration > 0 ? 0.2f : 0.9f);
                    if (hit.point.y < minHeight || hit.point.y > averageHeight + (playerStepHeight * 0.5f))
                    {
                        valid = false;
                    }
                    else if (h != hitCount - 1 && (hit.point.y + playerHeight > hits[h + 1].point.y || Physics.Raycast(hit.point, Vector3.up, playerHeight)))
                    {
                        valid = false;
                    }
                    else if (walkPointConfiguration < 0)
                    {
                        if (cylinders != null)
                            foreach (MathUtils.Cylinder c in cylinders)
                            {
                                if (c.ContrainsPoint(hit.point))
                                {
                                    valid = false;
                                    break;
                                }
                            }
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