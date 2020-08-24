using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuadViewer : MonoBehaviour
{
    private TextMeshPro tmp;
    private MeshRenderer mr;
    object quadTree;

    public static GameObject qvParent;
    public static int height;
    public static void InitViewer(int _height)
    {
        height = _height;
    }

    private void Awake()
    {   
        if(qvParent == null)
        {
            qvParent = new GameObject("QuadTreeViewers");
#if !IS_Y_AXIS_HEIGHT
            qvParent.transform.rotation = Quaternion.Euler(90, 0, 0);
#endif
        }

        tmp = GetComponentInChildren<TextMeshPro>();     
        mr = GetComponentInChildren<MeshRenderer>();
        mr.material.color = new Color(Random.Range(0,1f), Random.Range(0, 1f), Random.Range(0, 1f), 0.3f);
        this.transform.SetParent(qvParent.transform);
        this.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void SetQuadTree<T>(QuadTreeNode<T> qtn)
    {
        quadTree = qtn;
        mr.transform.localScale = new Vector3(qtn.boundary.width, qtn.boundary.height, 1);
        var newPos = qtn.boundary.center;// - qtn.boundary.size * 0.5f;

        tmp.gameObject.SetActive(qtn.IsSettedData);
        tmp.text = qtn.data.ToString();

#if !IS_Y_AXIS_HEIGHT
        transform.position = new Vector3(newPos.x, 0, newPos.y);
#else
        transform.position = new Vector3(newPos.x, 0, newPos.y);        
#endif



    }
}
