using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Triangle
{
    public Vector3 p1, p2, p3;
    //int [] indices;
    public Triangle(Vector3 _p1, Vector3 _p2, Vector3 _p3)//, int index1, int index2, int index3)
    {
        //indices = new int[3];
        p1 = _p1;
        p2 = _p2;
        p3 = _p3;
    }
}

public class GPUMove : MonoBehaviour
{
    public MeshFilter navMeshFilter;
    private Mesh navMesh;
    public ComputeShader computeShader;

    public Triangle[] triangles;

    ComputeBuffer computeBuffer;

    private void Start()
    {
        triangles = new Triangle[3];

        triangles[0] = new Triangle(Vector3.one, Vector3.one, Vector3.one);
        triangles[1] = new Triangle(Vector3.one * 2, Vector3.zero, Vector3.zero);
        triangles[2] = new Triangle(Vector3.one * 3, Vector3.zero, Vector3.zero);

        navMeshFilter = GameObject.Find("NavMesh").GetComponent<MeshFilter>();
        navMesh = navMeshFilter.mesh;

        //make buffer and set them triangles that i maded before
        //so we can use this values in compute shader threads        
        computeBuffer = new ComputeBuffer(1, triangles.GetByteSize<Triangle>());
        computeBuffer.SetData(triangles);
    }

    private void Update()
    {
        

        //you must match byte size to buffer
        Debug.LogError("Buffer.ByteLength : " + triangles.GetByteSize<Triangle>());

        //check before
        
        foreach(var tr in triangles)
        {
            Debug.LogError("--- tr p1: " + tr.p1+"\np2 : " + tr.p2 + "\np3: " + tr.p3 + "\n");
        }

        


        //load compute shader
        int kernelIndex = computeShader.FindKernel("CSMain");
        //set buffers for calc and results values
        computeShader.SetBuffer(kernelIndex, "Triangles", computeBuffer);
        //do dispatch
        computeShader.Dispatch(kernelIndex, triangles.Length, 1, 1);
        
        computeBuffer.GetData(triangles);


        int i = 0;
        Debug.LogError("--------------------------  after ----------------------");
        foreach (var tr in triangles)
        {
            var tempTr = triangles[i];
            tempTr.p1 = triangles[i].p1;
            tempTr.p2 = triangles[i].p2;
            tempTr.p3 = triangles[i].p3;

            triangles[i] = tempTr;
            i++;
            Debug.LogError(i+"nd --- tr p1: " + tr.p1 + "\np2 : " + tr.p2 + "\np3: " + tr.p3 + "\n");
        }
        Debug.LogError("--------------------------  end ----------------------");
    }

    //find trangle and position
    private void FindTriangle()
    {
        var triangles = navMesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var p1 = navMesh.vertices[triangles[i]];
            var p2 = navMesh.vertices[triangles[i + 1]];
            var p3 = navMesh.vertices[triangles[i + 2]];
        }
    }    
}
