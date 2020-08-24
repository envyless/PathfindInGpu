using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Mesh In CustomNavMesh, navmesh will be converted by This to QuadTree NavMesh
/// </summary>

public class MeshToQuadtree : MonoBehaviour
{
    Vector3 [] vertices;
    int[] indices;
    public MeshFilter mfTriangle;

    // Start is called before the first frame update
    void Start()
    {
        var mf = GetComponent<MeshFilter>();
        vertices = mf.sharedMesh.vertices;
        indices = mf.sharedMesh.GetIndices(0);

        DrawDebugLines();
    }

    void DrawDebugLines()
    {
        Mesh m = new Mesh();
        List<Vector3> meshVerts = new List<Vector3>();
        List<int> meshIndices = new List<int>();

        for (int i = 0; i < indices.Length - 2; i += 3)
        {
            var i0 = indices[i];
            var i1 = indices[i + 1];
            var i2 = indices[i + 2];

            var v0 = vertices[i0];
            var v1 = vertices[i1];
            var v2 = vertices[i2];

            LineDrawerMgr.DrawLine(v0, v1);
            LineDrawerMgr.DrawLine(v1, v2);
            LineDrawerMgr.DrawLine(v2, v0);

            if (i == 0)
            {
                meshVerts.Add(v0);
                meshVerts.Add(v1);
                meshVerts.Add(v2);
                meshIndices.Add(i0);
                meshIndices.Add(i1);
                meshIndices.Add(i2);
            }
        }

        CommonUtils.DrawLine(v2, v1);
    }

    Vector3 v1 = Vector3.zero;
    Vector3 v2 = Vector3.forward * 100;

    // Update is called once per frame
    void Update()
    {   
        if(Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < indices.Length - 2; i += 3)
            {
                var i0 = indices[i];
                var i1 = indices[i + 1];
                var i2 = indices[i + 2];

                var v0 = vertices[i0];
                var v1 = vertices[i1];
                var v2 = vertices[i2];


                bool isContain = CommonUtils.TriangleMethod.IsContain(v0, v1, v2, ScreenToWorldPlane.GetWorldPlanePos());
                if (isContain)
                {
                    Debug.LogError(i);
                    CommonUtils.DrawTriangle(v0, v1, v2, Color.red, 1, 1);
                }
            }
        }

        
    }
}
