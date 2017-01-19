using System;
using UnityEngine;

namespace Assets.TheTimeAgency.Scripts
{
    public class Triangle2D
    {
        private readonly Vector3[] _vertices;
        private double Area;
        private double Surface;

        public Triangle2D(Vector3 vec1, Vector3 vec2, Vector3 vec3)
        {
            if (vec1 == vec2 && vec1 == vec3) throw new ArgumentException("The Vector3's can't be the same!");

            _vertices = new[] { vec1, vec2, vec3 };

            Surface3D();
        }

        public Triangle2D(Vector3[] vertices) : this(vertices[0], vertices[1], vertices[2])
        {
            if (vertices.Length > 3) throw new ArgumentException("The Array accepts only 3 Vector3.");
        }

        private void Surface3D()
        {
            Vector3 p0 = _vertices[0];
            Vector3 p1 = _vertices[1];
            Vector3 p2 = _vertices[2];

            Area = 0.5 * (-p1.z * p2.x + p0.z * (-p1.x + p2.x) + p0.x * (p1.z - p2.z) + p1.x * p2.z);
        }

        public bool PointInTriangle(Vector3 p)
        {

            Vector3 p0 = _vertices[0];
            Vector3 p1 = _vertices[1];
            Vector3 p2 = _vertices[2];

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
            Vector3 first;
            Vector3 second;

            // gets the angle between the target section through all other points 
            var alpha = Vector3.Angle(_vertices[0] - target, _vertices[1] - target);
            var beta = Vector3.Angle(_vertices[0] - target, _vertices[2] - target);
            var gamma = Vector3.Angle(_vertices[1] - target, _vertices[2] - target);

            // the biggest angle is the correct one
            if (alpha > beta && alpha > gamma)
            {
                first = _vertices[0];
                second = _vertices[1];
            }
            else if (beta > alpha && beta > gamma)
            {
                first = _vertices[0];
                second = _vertices[2];
            }
            else
            {
                first = _vertices[1];
                second = _vertices[2];
            }

            return new Triangle2D(first, second, target);
        }

        public override string ToString()
        {
            return "Triangle2D: a:" + _vertices[0].ToString() + " b:" + _vertices[1].ToString() + " c:" + _vertices[2].ToString();
        }

        public double Sureface()
        {
            return Area;
        }

        public Vector3[] GetVertices()
        {
            return _vertices;
        }
    }
}
