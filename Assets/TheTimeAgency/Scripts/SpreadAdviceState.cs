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

        Transform startX;
        Transform endX;

        if (AB >= DC)
        {
            startX = ((GameObject)crimeScene.markerList[0]).transform;
            endX = ((GameObject)crimeScene.markerList[1]).transform;

            if (startX.position.x > endX.position.x)
            {
                endX = ((GameObject)crimeScene.markerList[0]).transform;
                startX = ((GameObject)crimeScene.markerList[1]).transform;
            }
        }
        else
        {
            startX = ((GameObject)crimeScene.markerList[2]).transform;
            endX = ((GameObject)crimeScene.markerList[3]).transform;

            if (startX.position.x > endX.position.x)
            {
                endX = ((GameObject)crimeScene.markerList[2]).transform;
                startX = ((GameObject)crimeScene.markerList[3]).transform;
            }
        }


        float AC = Vector3.Distance(((GameObject)crimeScene.markerList[1]).transform.localPosition, ((GameObject)crimeScene.markerList[2]).transform.localPosition);
        float BD = Vector3.Distance(((GameObject)crimeScene.markerList[0]).transform.localPosition, ((GameObject)crimeScene.markerList[3]).transform.localPosition);

        Transform startZ;
        Transform endZ;

        if (AC >= BD)
        {
            startZ = ((GameObject)crimeScene.markerList[1]).transform;
            endZ = ((GameObject)crimeScene.markerList[2]).transform;

            if (startZ.position.z > endZ.position.z)
            {
                endZ = ((GameObject)crimeScene.markerList[1]).transform;
                startZ = ((GameObject)crimeScene.markerList[2]).transform;
            }

        }
        else
        {
            startZ = ((GameObject)crimeScene.markerList[0]).transform;
            endZ = ((GameObject)crimeScene.markerList[3]).transform;

            if (startZ.position.z > endZ.position.z)
            {
                endZ = ((GameObject)crimeScene.markerList[0]).transform;
                startZ = ((GameObject)crimeScene.markerList[3]).transform;
            }
        }

        Mesh mesh = this.createPlaneMesh();

        float steps = 10.0f;

        ArrayList points = new ArrayList();

        for (float z = startZ.localPosition.z; z < endZ.localPosition.z; z += steps)
        {
            for (float x = startX.localPosition.x; x < endX.localPosition.x; x += steps)
            {

                if (this.IsPointInside(mesh, new Vector3(x, startX.localPosition.y, z)))
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

            if (P.GetSide(pt))
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

    public float getY(Vector3 point1, Vector3 point2, float x)
    {
        var m = (point2.z - point1.z) / (point2.x + point1.x);
        var b = point1.z - (m * point1.x);

        return m * x + b;
    }

    private void setEmptyAdvices()
    {

        if (crimeScene.m_cube.activeSelf != true)
        {
            Debug.Log("We will set empty GameObjects");

            Vector3 M = mittelVector(((GameObject)crimeScene.markerList[0]).transform.position, ((GameObject)crimeScene.markerList[1]).transform.position);
            GameObject myMarkerM = setAMarker("M");
            myMarkerM.transform.position = M;
            myMarkerM.GetComponent<Renderer>().material.color = Color.green;
            Vector3 P = new Vector3(((GameObject)crimeScene.markerList[3]).transform.position.x - M.x, ((GameObject)crimeScene.markerList[3]).transform.position.y, ((GameObject)crimeScene.markerList[3]).transform.position.z - M.z);
            GameObject myMarkerP = setAMarker("P");
            myMarkerP.transform.position = P;
            myMarkerP.GetComponent<Renderer>().material.color = Color.red;
            Vector3 Q = new Vector3(((GameObject)crimeScene.markerList[2]).transform.position.x + M.x, ((GameObject)crimeScene.markerList[2]).transform.position.y, ((GameObject)crimeScene.markerList[2]).transform.position.z + M.z);
            GameObject myMarkerQ = setAMarker("Q");
            myMarkerQ.transform.position = Q;
            myMarkerQ.GetComponent<Renderer>().material.color = Color.red;
            Vector3 N = mittelVector(P, Q);
            GameObject myMarkerN = setAMarker("N");
            myMarkerN.transform.position = N;
            myMarkerN.GetComponent<Renderer>().material.color = Color.green;




            crimeScene.m_cube.SetActive(true);

            float AM = Vector3.Distance(((GameObject)crimeScene.markerList[0]).transform.localPosition, myMarkerM.transform.localPosition);
            float DN = Vector3.Distance(((GameObject)crimeScene.markerList[2]).transform.localPosition, myMarkerN.transform.localPosition);

            Transform largerSide;
            Transform middlePoint;

            if (AM <= DN)
            {
                largerSide = ((GameObject)crimeScene.markerList[0]).transform;
                middlePoint = myMarkerM.transform;
            }
            else
            {
                largerSide = ((GameObject)crimeScene.markerList[2]).transform;
                middlePoint = myMarkerN.transform;
            }

            crimeScene.m_cube.transform.localPosition = new Vector3(
                    (myMarkerM.transform.localPosition.x + (myMarkerN.transform.localPosition.x - myMarkerM.transform.localPosition.x)) / 2,
                    myMarkerM.transform.localPosition.y,
                    (largerSide.localPosition.z + (myMarkerN.transform.localPosition.z - largerSide.localPosition.z)) / 2
                );

            Vector3 scale = crimeScene.m_cube.transform.localScale;

            scale.z = Vector3.Distance(myMarkerM.transform.localPosition, myMarkerN.transform.localPosition);
            scale.x = Vector3.Distance(largerSide.localPosition, middlePoint.localPosition);
            scale.y = 10;

            crimeScene.m_cube.transform.localScale = scale; // Find the distance between 2 points

            crimeScene.m_cube.transform.LookAt(myMarkerM.transform);




            //A = (1 / 2) |[(x2 - x0)(y3 - y1) + (x3 - x1)(y0 - y2)] |.


            float A = 0.5f * Math.Abs((((GameObject)crimeScene.markerList[2]).transform.localPosition.x - ((GameObject)crimeScene.markerList[0]).transform.localPosition.x) * (((GameObject)crimeScene.markerList[3]).transform.localPosition.z - ((GameObject)crimeScene.markerList[1]).transform.localPosition.z)
                                      + (((GameObject)crimeScene.markerList[3]).transform.localPosition.x - ((GameObject)crimeScene.markerList[1]).transform.localPosition.x) * (((GameObject)crimeScene.markerList[0]).transform.localPosition.z - ((GameObject)crimeScene.markerList[2]).transform.localPosition.z));

            Debug.Log("A" + ((GameObject)crimeScene.markerList[0]).transform.localPosition);
            Debug.Log("B" + ((GameObject)crimeScene.markerList[1]).transform.localPosition);
            Debug.Log("C" + ((GameObject)crimeScene.markerList[2]).transform.localPosition);
            Debug.Log("D" + ((GameObject)crimeScene.markerList[3]).transform.localPosition);
            Debug.Log("Flächeninhalt: " + A);
            Debug.Log("M" + myMarkerM.transform.localPosition);
            Debug.Log("N" + myMarkerN.transform.localPosition);
            Debug.Log("Mittlere Vector M <-> N " + ((myMarkerM.transform.localPosition + (myMarkerN.transform.localPosition - myMarkerM.transform.localPosition)) / 2));
            Debug.Log("Cube position" + crimeScene.m_cube.transform.position);
            Debug.Log("Cube rotation" + crimeScene.m_cube.transform.rotation);
            Debug.Log("Cube localScale" + crimeScene.m_cube.transform.localScale);
            Debug.Log("------------------------------------------------");

        }

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

    private GameObject setAMarker(string name)
    {
        // copy of the maker
        GameObject myMarker = GameObject.Instantiate<GameObject>(crimeScene.m_marker);

        myMarker.name = name;

        //http://answers.unity3d.com/questions/868484/why-is-instantiated-objects-scale-changing.html
        //Sets "m_marker Parent" as the new parent of the myMarker GameObject, except this makes the myMarker keep its local orientation rather than its global orientation.
        myMarker.transform.SetParent(crimeScene.m_marker.transform.parent.gameObject.transform, false);

        // Place the marker at the center of the screen at the found floor height.
        myMarker.SetActive(true);

        return myMarker;
    }



    private Vector3 mittelVector(Vector3 ls, Vector3 rs)
    {
        return (ls + rs) / 2;
    }
}
