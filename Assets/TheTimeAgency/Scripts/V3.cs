using UnityEngine;

public class V3
{
    public float x
    {
        get { return _vec3.x; }

        set
        {
            _vec3.x = value;
        }
    }

    public float y
    {
        get { return _vec3.y; }

        set
        {
            _vec3.y = value;
        }

    }

    public float z
    {
        get { return _vec3.z; }

        set
        {
            _vec3.z = value;
        }
    }

    public bool Checked { get; set; }

    private Vector3 _vec3;

    public Vector3 vec3
    {
        get { return _vec3; }
    }

    public V3(Vector3 v3)
    {
        _vec3 = v3;
    }
}
