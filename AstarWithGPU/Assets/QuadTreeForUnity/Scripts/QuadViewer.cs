#define IS_Y_AXIS_HEIGHT
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuadViewer : MonoBehaviour
{
    private TextMeshPro tmp;
    private MeshRenderer mr;
    object quadTree;

    private void Awake()
    {       
        tmp = GetComponentInChildren<TextMeshPro>();     
        mr = GetComponentInChildren<MeshRenderer>();
        mr.material.color = new Color(Random.Range(0,1f), Random.Range(0, 1f), Random.Range(0, 1f), 0.3f);
    }

    public void SetQuadTree<T>(QuadTreeNode<T> qtn)
    {
        quadTree = qtn;
        mr.transform.localScale = new Vector3(qtn.boundary.width, qtn.boundary.height, 1);
        var newPos = qtn.boundary.center;// - qtn.boundary.size * 0.5f;
#if IS_Y_AXIS_HEIGHT
        transform.position = new Vector3(newPos.x, newPos.y);
#else
        transform.position = new Vector3(newPos.x, 0, newPos.y);
#endif
        tmp.text = qtn.data.ToString();
    }
}
