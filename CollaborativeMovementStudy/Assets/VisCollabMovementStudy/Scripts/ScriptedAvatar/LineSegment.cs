using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSegment
{
    public Transform A;
    public Transform B;

    public float ShortestDistance(Vector2 a_e)
    {
        return LineSegment.ShortestDistance(
            LineSegment.ProjectFlat(A.position),
            LineSegment.ProjectFlat(B.position),
            a_e
            );
    }

    public static Vector2 ProjectFlat(Transform a_v)
    {
        return ProjectFlat(a_v.position);

    }
    public static Vector2 ProjectFlat(Vector3 a_v)
    {
        return new Vector2(a_v.x, a_v.z);
    }

    //public static float ShortestDistanceUnity(Vector2 a_a, Vector2 a_b, Vector2 a_e)
    //{
    //    // only available in editor. Not at compile time.
    //    return HandleUtility.DistancePointToLineSegment(a_e, a_a, a_b);
    //}

    public static float ShortestDistance(Vector2 a_a, Vector2 a_b, Vector2 a_e)
    {
        Vector2 AB = a_b - a_a;
        Vector2 BE = a_e - a_b;
        Vector2 AE = a_e - a_a;

        float AB_BE = Vector2.Dot(AB, BE);
        float AB_AE = Vector2.Dot(AB, AE);

        //d_aIsNearest = false;
        //d_bIsNearest = false;

        float ret = 0.0f;
        if (AB_BE > 0)
        {
            //d_bIsNearest = true;
            ret = Vector2.Distance(a_b, a_e);
        }
        else if (AB_AE < 0)
        {
            //d_aIsNearest = true;
            ret = Vector2.Distance(a_a, a_e);
        }
        else
        {
            // between A and B -> shortest point on line segment
            float mod = Mathf.Sqrt(AB.x * AB.x + AB.y * AB.y);
            ret = Mathf.Abs(AB.x * AE.y - AB.y * AE.x) / mod;
            //// Finding the perpendicular distance
            //double x1 = AB.F;
            //double y1 = AB.S;
            //double x2 = AE.F;
            //double y2 = AE.S;
            //double mod = Math.Sqrt(x1 * x1 + y1 * y1);
            //reqAns = Math.Abs(x1 * y2 - y1 * x2) / mod;
        }

        return ret;
    }
}

