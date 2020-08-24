using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //init size
        QuadTreeNode<int>.Init(Vector3.zero, Vector2.one * 100, 100);        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var pos = ScreenToWorldPlane.GetWorldPlanePos();
            QuadTreeNode<int>.Root.Insert(pos, (int)pos.x);
        }        
    }
}
