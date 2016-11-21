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

        public Triangle2D AdjacentTriangle(Vector3 target)
        {

            var dist1 = DistToSegment(target, _vecArray[0], _vecArray[1]);
            var dist2 = DistToSegment(target, _vecArray[1], _vecArray[2]);
            var dist3 = DistToSegment(target, _vecArray[2], _vecArray[0]);

            if (dist1 <= dist2 && dist1 <= dist3)
            {
                return new Triangle2D(_vecArray[0], _vecArray[1], target);
            }
            if (dist2 <= dist1 && dist2 <= dist3)
            {
                return new Triangle2D(_vecArray[1], _vecArray[2], target);
            }

            return new Triangle2D(_vecArray[2], _vecArray[0], target);
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
    }
}
