using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Circle
{
    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius)
    {
        this.center = center;
        this.radius = radius;
    }

    public int GetIntersectCount(Vector2 lineStart, Vector2 lineEnd)
    {
        // Calculate intersection points
        Vector2 d = lineEnd - lineStart;
        Vector2 f = lineStart - center;

        float a = Vector2.Dot(d, d);
        float b = 2 * Vector2.Dot(f, d);
        float c = Vector2.Dot(f, f) - radius * radius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            // No intersection
            return 0;
        }
        else if (discriminant == 0)
        {
            // One intersection, check if it's within the segment
            float t = -b / (2 * a);
            if (t >= 0 && t <= 1)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            // Two intersections, check if they are within the segment
            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2 * a);
            float t2 = (-b - sqrtDiscriminant) / (2 * a);

            int intersections = 0;

            if (t1 >= 0 && t1 <= 1)
            {
                intersections++;
            }

            if (t2 >= 0 && t2 <= 1)
            {
                intersections++;
            }

            return intersections;
        }
    }

}
