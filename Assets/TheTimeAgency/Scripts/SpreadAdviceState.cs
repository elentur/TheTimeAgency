using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.TheTimeAgency.Scripts;

public class SpreadAdviceState : ICrimeSceneState
{
    private readonly CrimeScene crimeScene;

    public SpreadAdviceState(CrimeScene crimeScenePattern)
    {
        crimeScene = crimeScenePattern;
    }

    void ICrimeSceneState.UpdateState()
    {

        crimeScene.markerList.Sort((IComparer) new ClockwiseVector3Comparer());

        float AB = Vector3.Distance(((GameObject)crimeScene.markerList[0]).transform.localPosition, ((GameObject)crimeScene.markerList[1]).transform.localPosition);
        float DC = Vector3.Distance(((GameObject)crimeScene.markerList[3]).transform.localPosition, ((GameObject)crimeScene.markerList[2]).transform.localPosition);

        Transform startZ;
        Transform endZ;

        if (AB >= DC)
        {
            startZ = ((GameObject)crimeScene.markerList[0]).transform;
            endZ = ((GameObject)crimeScene.markerList[1]).transform;

            if (startZ.position.z > endZ.position.z)
            {
                endZ = ((GameObject)crimeScene.markerList[0]).transform;
                startZ = ((GameObject)crimeScene.markerList[1]).transform;
            }
        }
        else
        {
            startZ = ((GameObject)crimeScene.markerList[2]).transform;
            endZ = ((GameObject)crimeScene.markerList[3]).transform;

            if (startZ.position.z > endZ.position.z)
            {
                endZ = ((GameObject)crimeScene.markerList[2]).transform;
                startZ = ((GameObject)crimeScene.markerList[3]).transform;
            }
        }


        float AC = Vector3.Distance(((GameObject)crimeScene.markerList[1]).transform.localPosition, ((GameObject)crimeScene.markerList[2]).transform.localPosition);
        float BD = Vector3.Distance(((GameObject)crimeScene.markerList[0]).transform.localPosition, ((GameObject)crimeScene.markerList[3]).transform.localPosition);

        Transform startX;
        Transform endX;

        if (AC >= BD)
        {
            startX = ((GameObject)crimeScene.markerList[1]).transform;
            endX = ((GameObject)crimeScene.markerList[2]).transform;

            if (startX.position.x > endZ.position.x)
            {
                endX = ((GameObject)crimeScene.markerList[1]).transform;
                startX = ((GameObject)crimeScene.markerList[2]).transform;
            }

        }
        else
        {
            startX = ((GameObject)crimeScene.markerList[0]).transform;
            endX = ((GameObject)crimeScene.markerList[3]).transform;

            if (startX.position.x > endX.position.x)
            {
                endX = ((GameObject)crimeScene.markerList[0]).transform;
                startX = ((GameObject)crimeScene.markerList[3]).transform;
            }
        }

        Mesh mesh = this.createPlaneMesh();

        float steps = 10.0f;

        ArrayList points = new ArrayList();

        for (float z = startZ.localPosition.z; z < endZ.localPosition.z; z += steps)
        {
            for (float x = startX.localPosition.x; x < endX.localPosition.x; x += steps)
            {

                if (IsPointInside(mesh, new Vector3(x, startX.localPosition.y, z)))
                {

                    GameObject cube = setACube(x + "/" + z);

                    cube.transform.localPosition = new Vector3(x, startX.localPosition.y, z);

                    cube.transform.localScale = new Vector3(steps - steps / steps, steps - steps / steps, steps - steps / steps);

                    points.Add(cube.transform.localPosition);

                }
            }
        }


        Debug.Log("------------Points Order---------------");
        Debug.Log(((GameObject)crimeScene.markerList[0]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[1]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[2]).transform.localPosition);
        Debug.Log(((GameObject)crimeScene.markerList[3]).transform.localPosition);
        Debug.Log("------------founded x line / z line---------------");
        Debug.Log(startX.localPosition);
        Debug.Log(endX.localPosition);
        Debug.Log(startZ.localPosition);
        Debug.Log(endZ.localPosition);
        Debug.Log("Arrays: " + points.Count);
        Debug.Log("---------------------------");

        ToPingState();
    }

    void ICrimeSceneState.OnGUIState()
    {

    }

    public class ClockwiseVector3Comparer : IComparer
    {
        public int Compare(object obj1, object obj2)
        {
            Vector3 v1 = ((GameObject)obj1).transform.localPosition;
            Vector3 v2 = ((GameObject)obj2).transform.localPosition;

            return Mathf.Atan2(v1.x, v1.z).CompareTo(Mathf.Atan2(v2.x, v2.z));
        }
    }

    private void ToPingState()
    {
        crimeScene.currentState = crimeScene.pingState;
    }

    private bool isPointInside(Vector3 target)
    {
        var xArray = new float[crimeScene.markerList.Count];
        var zArray = new float[crimeScene.markerList.Count];

        for (int i = 0; i < crimeScene.markerList.Count; i++)
        {
            xArray[i] = ((GameObject)crimeScene.markerList[i]).transform.position.x;
            zArray[i] = ((GameObject)crimeScene.markerList[i]).transform.position.z;
        }

        var maxX = xArray.Max();
        var minX = xArray.Min();
        var maxZ = zArray.Max();
        var minZ = zArray.Min();

        return target.x >= minX && target.x <= maxX && target.z >= minZ && target.z <= maxZ;
    }

    public bool IsPointInside(Mesh aMesh, Vector3 pt)
    {
        Vector3[] verts = aMesh.vertices;
        int[] tris = aMesh.triangles;
        int triangleCount = tris.Length / 3;

        bool b1, b2, b3, inside = false;

        for (int i = 0; i < triangleCount; i++)
        {
            Vector3 V1 = verts[tris[i * 3]];
            Vector3 V2 = verts[tris[i * 3 + 1]];
            Vector3 V3 = verts[tris[i * 3 + 2]];

            var P = new Plane(V1,V2,V3);

            if (P.GetSide(pt) && !inside)
                inside = true;
        }
        return inside;
    }

    private Mesh createPlaneMesh()
    {
        Mesh mesh = new Mesh();

        crimeScene.gameObject.AddComponent<MeshRenderer>();
        crimeScene.gameObject.AddComponent<MeshFilter>().mesh = mesh;

        // TODO kontrollieren ob die Dreiecke in der richtigen reihenfloge gesetz werden!

        Vector3[] vertices = new Vector3[]
        {
                 ((GameObject)crimeScene.markerList[0]).transform.localPosition,
                 ((GameObject)crimeScene.markerList[1]).transform.localPosition,
                 ((GameObject)crimeScene.markerList[2]).transform.localPosition,
                 ((GameObject)crimeScene.markerList[3]).transform.localPosition,
        };

        int[] triangles = new int[]
        {
                 0, 1, 2,
                 0, 2, 3,
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    private GameObject setACube(string name)
    {
        // copy of the maker
        GameObject myCube = GameObject.Instantiate<GameObject>(crimeScene.m_cube);

        myCube.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        myCube.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

        // Place the marker at the center of the screen at the found floor height.
        myCube.SetActive(true);

        return myCube;
    }
}
