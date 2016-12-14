using System;
using System.Linq;
using UnityEngine;

namespace Assets.TheTimeAgency.Scripts
{
    public class Triangle2D
    {
        private readonly Vector3[] _vecArray;
        private double Area;
        private double Surface;

        public Triangle2D(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            this._vecArray = new[] {vec1, vec2, vec3};

            Vector3 p0 = _vecArray[0];
            Vector3 p1 = _vecArray[1];
            Vector3 p2 = _vecArray[2];

            Surface3D(p0, p1, p2);
        }

        public Triangle2D(Vector3[] vecArray)
        {
            if (vecArray.Length > 3) throw new ArgumentException("The Array accepts only 3 Vector3.");
            this._vecArray = vecArray;

            Vector3 p0 = _vecArray[0];
            Vector3 p1 = _vecArray[1];
            Vector3 p2 = _vecArray[2];

            Surface3D(p0, p1, p2);
        }

        private void Surface3D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Area = 0.5 * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
        }

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

            if (_vecArray.Length > 3) throw new ArgumentException("The AdjacentTriangle only works with max. 3 Points!");

            Vector3 first;
            Vector3 second;

            // gets the angle between the target section through all other points 
            var alpha = Vector3.Angle(_vecArray[0] - target, _vecArray[1] - target);
            var beta = Vector3.Angle(_vecArray[0] - target, _vecArray[2] - target);
            var gamma = Vector3.Angle(_vecArray[1] - target, _vecArray[2] - target);

            // the biggest angle is the correct one
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
        }

        public override string ToString()
        {
            return "Triangle2D: a:" + _vecArray[0].ToString() + " b:" + _vecArray[1].ToString() + " c:" + _vecArray[2].ToString();
        }

        public double Sureface()
        {
            return Area;
        }
    }
}
