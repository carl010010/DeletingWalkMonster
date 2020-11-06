using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        foreach (float p in tLeft)
        {
            points.Clear();
            configuration = 0;


            points.Add(topLeft.postition + Vector3.up * p);
            configuration += 8;// 8 = top left


            foreach (float p1 in tRight)
            {
                if (Mathf.Abs(p - p1) < playerStepHeight)
                {
                    points.Add(topRight.postition + Vector3.up * p1);
                    configuration += 4;// 4 = top right
                    tRight.Remove(p1);
                    break;
                }
            }

            foreach (float p2 in bRight)
            {
                if (Mathf.Abs(p - p2) < playerStepHeight)
                {
                    points.Add(bottomRight.postition + Vector3.up * p2);
                    configuration += 2;// 2 = bottom right
                    bRight.Remove(p2);
                    break;
                }
            }

            foreach (float p3 in bLeft)
            {
                if (Mathf.Abs(p - p3) < playerStepHeight)
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

            walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
        }

        foreach (float p in tRight)
        {
            points.Clear();
            configuration = 0;

            points.Add(topRight.postition + Vector3.up * p);
            configuration += 4;// 4 = top right

            foreach (float p1 in bRight)
            {
                if (Mathf.Abs(p - p1) < playerStepHeight)
                {
                    points.Add(bottomRight.postition + Vector3.up * p1);
                    configuration += 2;// 2 = bottom right
                    bRight.Remove(p1);
                    break;
                }
            }

            foreach (float p2 in bLeft)
            {
                if (Mathf.Abs(p - p2) < playerStepHeight)
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

            walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
        }

        foreach (float p in bRight)
        {
            points.Clear();
            configuration = 0;

            points.Add(bottomRight.postition + Vector3.up * p);
            configuration += 2;// 2 = bottom right

            foreach (float p1 in bLeft)
            {
                if (Mathf.Abs(p - p1) < playerStepHeight)
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

            walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
        }

        foreach (float p in bLeft)
        {
            points.Clear();
            configuration = 0;

            points.Add(bottomLeft.postition + Vector3.up * p);
            configuration += 1;// 1 = bottom left

            walkGroups.Add(new WalkPointGroup(points.ToArray(), configuration, pollSpacing, playerHeight, playerStepHeight, cylinders));
        }
    }
}
