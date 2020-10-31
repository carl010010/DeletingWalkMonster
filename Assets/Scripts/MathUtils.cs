using UnityEngine;

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
}
