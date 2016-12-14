using UnityEngine;

namespace Assets.TheTimeAgency.Scripts
{
    public struct V3
    {
        public float x, y, z;
        public Vector3 Vec3;
        public bool Examined;

        public V3(Vector3 v3)
        {
            Vec3 = v3;
            x = Vec3.x;
            y = Vec3.y;
            z = Vec3.z;
            Examined = false;
        }
    }
}
