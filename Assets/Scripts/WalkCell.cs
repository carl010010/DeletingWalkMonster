using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace test
{
    public class WalkCell
    {
        public List<WalkPointGroup> walkGroups = new List<WalkPointGroup>();

        public WalkCell(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft, float pollSpacing, float playerHeight, float playerStepHeight, List<MathUtils.Cylinder> cylinders = null)
        {
            FillWalkPoints(topLeft, topRight, bottomRight, bottomLeft, pollSpacing, playerHeight, playerStepHeight, cylinders);
        }

        void FillWalkPoints(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft, float pollSpacing, float playerHeight, float playerStepHeight, List<MathUtils.Cylinder> cylinders)
        {
            List<Vector3> points = new List<Vector3>(8);

            List<float> tLeft = new List<float>(topLeft.yHeights);
            List<float> tRight = new List<float>(topRight.yHeights);
            List<float> bRight = new List<float>(bottomRight.yHeights);
            List<float> bLeft = new List<float>(bottomLeft.yHeights);

            short configuration = 0;
            float height = 0;
            short retConfig = 0;
            float retHeight = 0;
            short ConfigCount = 1;

            foreach (float p in tLeft)
            {
                points.Clear();
                configuration = 0;
                height = 0;
                retConfig = 0;
                retHeight = 0;
                ConfigCount = 1;


                points.Add(topLeft.postition + Vector3.up * p);
                configuration += 8;// 8 = top left
                height = p;

                // 4 = top right
                (retConfig, retHeight) = HasPointInWalkGroup(ref tRight, topRight.postition, ref points, p, 4, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;

                // 2 = bottom right
                (retConfig, retHeight) = HasPointInWalkGroup(ref bRight, bottomRight.postition, ref points, p, 2, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;

                // 1 = bottom left
                (retConfig, retHeight) = HasPointInWalkGroup(ref bLeft, bottomLeft.postition, ref points, p, 1, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;


                height /= ConfigCount;

                if ((configuration & 4) != 4)
                {
                    (retConfig, retHeight) = HasPointInWalkGroup(ref tRight, topRight.postition, ref points, height, 4, playerStepHeight);
                    configuration += retConfig;
                    if (retConfig > 0) ConfigCount++;
                    height += retHeight;
                }

                if ((configuration & 2) != 2)
                {
                    (retConfig, retHeight) = HasPointInWalkGroup(ref bRight, bottomRight.postition, ref points, height, 2, playerStepHeight);
                    configuration += retConfig;
                    if (retConfig > 0) ConfigCount++;
                    height += retHeight;
                }

                if ((configuration & 1) != 1)
                {
                    (retConfig, retHeight) = HasPointInWalkGroup(ref bLeft, bottomLeft.postition, ref points, height, 1, playerStepHeight);
                    configuration += retConfig;
                    if (retConfig > 0) ConfigCount++;
                    height += retHeight;
                }

                if (configuration != 15)
                {
                    configuration = Contains_A_BlockedWalkEdge(topLeft, topRight, bottomRight, bottomLeft, playerStepHeight, points, configuration);
                }
                if (points.Count > 2)
                {
                    points.Add(topLeft.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
            }

            foreach (float p in tRight)
            {
                points.Clear();
                configuration = 0;
                height = 0;
                retConfig = 0;
                retHeight = 0;
                ConfigCount = 1;

                points.Add(topRight.postition + Vector3.up * p);
                configuration += 4;// 4 = top right

                // 2 = bottom right
                (retConfig, retHeight) = HasPointInWalkGroup(ref bRight, bottomRight.postition, ref points, p, 2, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;

                // 1 = bottom left
                (retConfig, retHeight) = HasPointInWalkGroup(ref bLeft, bottomLeft.postition, ref points, p, 1, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;

                configuration = Contains_A_BlockedWalkEdge(topLeft, topRight, bottomRight, bottomLeft, playerStepHeight, points, configuration);

                if (points.Count > 2)
                {
                    points.Add(topRight.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
            }

            foreach (float p in bRight)
            {
                points.Clear();
                configuration = 0;
                height = 0;
                retConfig = 0;
                retHeight = 0;
                ConfigCount = 1;

                points.Add(bottomRight.postition + Vector3.up * p);
                configuration += 2;// 2 = bottom right

                // 1 = bottom left
                (retConfig, retHeight) = HasPointInWalkGroup(ref bLeft, bottomLeft.postition, ref points, p, 1, playerStepHeight);
                configuration += retConfig;
                if (retConfig > 0) ConfigCount++;
                height += retHeight;

                configuration = Contains_A_BlockedWalkEdge(topLeft, topRight, bottomRight, bottomLeft, playerStepHeight, points, configuration);

                if (points.Count > 2)
                {
                    points.Add(bottomRight.postition + Vector3.up * p);
                }

                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
            }

            foreach (float p in bLeft)
            {
                points.Clear();
                configuration = 0;

                points.Add(bottomLeft.postition + Vector3.up * p);
                configuration += 1;// 1 = bottom left

                configuration = Contains_A_BlockedWalkEdge(topLeft, topRight, bottomRight, bottomLeft, playerStepHeight, points, configuration);


                walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
            }
        }

        // pointConfiguration:
        // 8 = top left
        // 4 = top right
        // 2 = bottom right
        // 1 = bottom left
        private static (short configuration, float pointHeight)
                        HasPointInWalkGroup(ref List<float> pollPoints, Vector3 pollPostition, ref List<Vector3> points, float p, short pointConfiguration, float playerStepHeight)
        {
            foreach (float p1 in pollPoints)
            {
                if (Mathf.Abs(p - p1) < playerStepHeight)
                {
                    points.Add(pollPostition + Vector3.up * p1);
                    pollPoints.Remove(p1);
                    return (pointConfiguration, p1);
                }
            }

            return (0, 0);
        }

        private static short Contains_A_BlockedWalkEdge(Poll topLeft, Poll topRight, Poll bottomRight, Poll bottomLeft, float playerStepHeight, List<Vector3> points, short configuration)
        {
            for (int i = 0; i < points.Count && configuration > 0; i++)
            {
                Vector3 v = points[i];
                configuration = ContainsBlockedWalkPoint(topLeft, configuration, v);

                if (configuration < 0)
                    break;

                configuration = ContainsBlockedWalkPoint(topRight, configuration, v);

                if (configuration < 0)
                    break;

                configuration = ContainsBlockedWalkPoint(bottomRight, configuration, v);

                if (configuration < 0)
                    break;

                configuration = ContainsBlockedWalkPoint(bottomLeft, configuration, v);
            }

            return configuration;
        }

        private static short ContainsBlockedWalkPoint(Poll poll, short configuration, Vector3 v)
        {
            foreach (float f in poll.yBlockedHeights)
            {
                if (v.y <= f + 0.1)
                {
                    configuration *= -1;
                    break;
                }
            }

            return configuration;
        }
    }
}