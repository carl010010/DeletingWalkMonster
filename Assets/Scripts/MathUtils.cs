﻿using UnityEngine;

public class MathUtils
{
    public static RaycastHit[] RaycastHitSortByY(RaycastHit[] hits, int count)
    {
		RaycastHit temp;
		int j;

		for (int i = 1; i < count; i++)
		{
			temp = hits[i];
			for (j = i; j > 0 && temp.point.y < hits[j - 1].point.y; j--)
			{
				hits[j] = hits[j - 1];
			}
			hits[j] = temp;
		}
		return hits;
    }


	public struct Cylinder
    {
		public Vector3 pos;
		public float stepHeight;
		public float raduis;

        public Cylinder(Vector3 pos, float stepHeight, float raduis)
        {
            this.pos = pos;
            this.stepHeight = stepHeight;
            this.raduis = raduis;
        }

		public bool ContrainsPoint(Vector3 point)
        {
			Vector2 pos2d = new Vector2(pos.x, pos.z);
			Vector2 point2d = new Vector2(point.x, point.z);

			if (point.y < pos.y - stepHeight || point.y > pos.y + stepHeight ||
				(pos2d - point2d).sqrMagnitude > raduis * raduis)
            {
				return false;
            }


			return true;
        }
    }
}
