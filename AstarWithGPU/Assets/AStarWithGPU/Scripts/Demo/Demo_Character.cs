using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Demo_Character : MonoBehaviour
{
    //using this, this character will find path
    public NavMeshData navMeshData;
    public NavMeshAgent nma;

    public static Mesh myNavMesh;
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("navMD : " + navMeshData);
        navMeshData.position = transform.position;
        //NavMeshToCustomArray();
    }
    void NavMeshToCustomArray()
    {
        myNavMesh = GameObject.Find("CustomNavMesh").GetComponent<MeshFilter>().mesh;        
    }

    private void Update()
    {
        Graphics.DrawMesh(myNavMesh, Matrix4x4.identity, mat, 1);
    }


}
