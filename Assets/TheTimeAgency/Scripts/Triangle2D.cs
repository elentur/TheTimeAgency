using System;
using System.Linq;
using UnityEngine;

namespace Assets.TheTimeAgency.Scripts
{
    public class Triangle2D
    {
        private readonly Vector3[] _vecArray;
        private double Area;

        public Triangle2D(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            this._vecArray = new[] {vec1, vec2, vec3};

            Vector3 p0 = _vecArray[0];
            Vector3 p1 = _vecArray[1];
            Vector3 p2 = _vecArray[2];

            Area = 0.5 * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
        }

        public Triangle2D(Vector3[] vecArray)
        {
            if (vecArray.Length > 3) throw new ArgumentException("The Array accepts only 3 Vector3.");
            this._vecArray = vecArray;

            Vector3 p0 = _vecArray[0];
            Vector3 p1 = _vecArray[1];
            Vector3 p2 = _vecArray[2];

            Area = 0.5 * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
        }

        private static float Sign(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return (v1.x - v3.x)*(v2.z - v3.z) - (v2.x - v3.x)*(v1.z - v3.z);
        }

        /*public bool PointInTriangle(Vector3 target)
        {
            bool b1, b2, b3;

            b1 = Sign(target, _vecArray[0], _vecArray[1]) < 0.0f;
            b2 = Sign(target, _vecArray[1], _vecArray[2]) < 0.0f;
            b3 = Sign(target, _vecArray[2], _vecArray[0]) < 0.0f;

            return (b1 == b2) && (b2 == b3);
        }*/

        public bool PointInTriangle(Vector3 p)
        {
            Vector3 p0 = _vecArray[0];
            Vector3 p1 = _vecArray[1];
            Vector3 p2 = _vecArray[2];

            double s = 1.0 / (2.0 * Area) * (p0.z * p2.x - p0.x * p2.z + (p2.z - p0.z) * p.x + (p0.x - p2.x) * p.z);
            double t = 1.0 / (2.0 * Area) * (p0.x * p1.z - p0.z * p1.x + (p0.z - p1.z) * p.x + (p1.x - p0.x) * p.z);
            if (s + t <= 1)
            {
                return s >= 0 && t >= 0;
            }
            return false;
        }

        private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
        {
            Vector2 diference = vec2 - vec1;
            float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
            return Vector2.Angle(Vector2.right, diference) * sign;
        }

        public Triangle2D AdjacentTriangle(Vector3 target)
        {

            Vector3 first;
            Vector3 second;

            // checks the angle between the target section through all other points 
            float alpha = Vector3.Angle(_vecArray[0] - target, _vecArray[1] - target);
            float beta = Vector3.Angle(_vecArray[0] - target, _vecArray[2] - target);
            float gamma = Vector3.Angle(_vecArray[1] - target, _vecArray[2] - target);

            if (alpha > beta && alpha > gamma)
            {
                first = _vecArray[0];
                second = _vecArray[1];
            }else if (beta > alpha && beta > gamma)
            {
                first = _vecArray[0];
                second = _vecArray[2];
            }
            else
            {
                first = _vecArray[1];
                second = _vecArray[2];
            }

            return new Triangle2D(first, second, target);

            /*Vector2 p = new Vector2(target.x, target.z);

            Debug.Log(string.Format("point: {0}", p));

            int len = _vecArray.Length;

            int[] myIndex = new int[2];
            int j = 0;

            for (var i = 0; i < len; i++)
            {
                Vector3 current = new Vector2(_vecArray[i % len].x, _vecArray[i % len].z);
                Vector3 next = new Vector2(_vecArray[(i+1) % len].x, _vecArray[(i+1) % len].z);
                Vector3 nextAfter = new Vector2(_vecArray[(i+2) % len].x, _vecArray[(i+2) % len].z);

                if (!Intersection(p, current, current, next) && !Intersection(p, current, next, nextAfter) &&
                    !Intersection(p, current, nextAfter, current))
                {
                    myIndex[j] = i;
                    j++;
                }
            }

            Debug.Log(string.Format("myIndex: {0}", myIndex));

            return new Triangle2D(_vecArray[myIndex[0]], _vecArray[myIndex[1]], target);*/
        }

        private static double Sqr(float x)
        {
            return x*x;
        }
        private static double Dist2(Vector3 v, Vector3 w)
        {
            return Sqr(v.x - w.x) + Sqr(v.z - w.z);
        }
        private static double DistToSegmentSquared(Vector3 p, Vector3 v, Vector3 w)
        {
            var l2 = Dist2(v, w);
            if (l2 == 0) return Dist2(p, v);
            var t = ((p.x - v.x) * (w.x - v.x) + (p.z - v.z) * (w.z - v.z)) / l2;
            t = Math.Max(0, Math.Min(1, t));
            return Dist2(p, new Vector3(
                (float) (v.x + t * (w.x - v.x)),
                v.y, 
                (float) (v.z + t * (w.z - v.z)) 
                )
                );
        }

        private double DistToSegment(Vector3 p, Vector3 v, Vector3 w)
        {
            return Math.Sqrt(DistToSegmentSquared(p, v, w));
        }

        public override string ToString()
        {
            return "Triangle2D: a:" + _vecArray[0].ToString() + " b:" + _vecArray[1].ToString() + " c:" + _vecArray[2].ToString();
        }

        private bool Intersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            //TODO Verbinde punkt 4 mit jedem der 3 anderen und prüfe die enstandene Gerade 
            //mit allen geraden des dreicks, nim die ersten beiden treffer
            float A1 = p1.y - p2.y;
            float B1 = p2.x - p1.x;
            float C1 = A1 * p2.x + B1 * p2.y;

            // Get A,B,C of second line - points : ps2 to pe2
            float A2 = q1.y - q2.y;
            float B2 = q2.x - q1.x;
            float C2 = A2 * q2.x + B2 * q2.y;


            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0)
                return false;

            // now return the Vector2 intersection point
            Vector2 intersect = new Vector2(
                (B2 * C1 - B1 * C2) / delta,
                (A1 * C2 - A2 * C1) / delta
            );

            if (intersect == p1 || intersect == p2 || intersect == q1 || intersect == q2) return false;

            if (Vector2.Distance(p1, intersect) + Vector2.Distance(p2, intersect) == Vector2.Distance(p1, p2) ||
                Vector2.Distance(q2, intersect) + Vector2.Distance(q1, intersect) == Vector2.Distance(q2, q1))
                return true; // C is on the line.
            return false;


        }


    }
}
