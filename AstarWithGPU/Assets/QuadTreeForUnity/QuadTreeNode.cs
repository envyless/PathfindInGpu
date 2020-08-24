#define IS_Y_AXIS_HEIGHT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * this quad tree purpose, save point
 * and get data from that point contains T
 * T could be anything 
 */

public unsafe class QuadTreeNode<T>
{
    public static QuadTreeNode<T> Root;
    public const int MaxCapacity = 4;
    public const int MaxDepth = 8;

    public QuadTreeNode<T> Parent;
    
    //bucket
    //data
    public T data;
    public Vector3 dataPosition;
    private bool IsSettedData = false;

    public uint currentCapacity = 0;

    QuadTreeNode<T> parent;

    //childrens
    QuadTreeNode<T> childLeftTop;
    QuadTreeNode<T> childRightTop;
    QuadTreeNode<T> childLeftDown;
    QuadTreeNode<T> childRightDown;

    public Rect boundary = new Rect();

    public enum WhereIs
    {
        LD = 0,
        RD = 1,
        LT = 2,
        RT = 3
    }

    public (QuadTreeNode<T>, WhereIs) WhereIsIn(Vector2 pos)
    {
        //related position
        pos += boundary.size * 0.5f;
        var x = (int)(pos.x / (boundary.width * 0.5f));
        var y = (int)(pos.y / (boundary.height * 0.5f));
        var childTarget = childLeftDown;
        var whereIs = (WhereIs)(x + y * 2);
        switch (whereIs)
        {
            case WhereIs.LD:
                childTarget = childLeftDown;
                break;
            case WhereIs.RD:
                childTarget = childRightDown;
                break;
            case WhereIs.LT:
                childTarget = childLeftTop;
                break;
            case WhereIs.RT:
                childTarget = childRightTop; 
                break;
        }
        Debug.LogError("whereIs : "+whereIs);
        return (childLeftDown, whereIs);
    }

    public bool Insert(Vector3 _pos, T _data)
    {
        if (!IsIn(_pos))
        {
            Debug.LogError(_pos+"is in : " + false);
            return false;
        }
        

        if(IsSettedData)
        {
            //if i need children?
            //do children aloc
            //and insert them
            MakeChild();
            if (childLeftDown.Insert(_pos, _data))
                return true;
            else if (childLeftTop.Insert(_pos, _data))
                return true;
            else if (childRightDown.Insert(_pos, _data))
                return true;
            else if (childRightTop.Insert(_pos, _data))
                return true;
            return false;
        }
        else
        {
            this.IsSettedData = true;
            this.dataPosition = _pos;
            this.data = _data;
            SetUpViewer();
        }
        return true;
    }

    bool IsHaveChild = false;
    private void MakeChild()
    {
        if (IsHaveChild)
            return;
        IsHaveChild = true;
        childLeftDown = new QuadTreeNode<T>(this, 0);
        childRightDown = new QuadTreeNode<T>(this, 1);
        childLeftTop = new QuadTreeNode<T>(this, 2);
        childRightTop = new QuadTreeNode<T>(this, 3);
    }


    private bool IsIn(Vector2 pos)
    {
        return boundary.Contains(pos);
    }

    private void Subdivide()
    {
        //when over capacity
    }

    QuadViewer qv;
    private void SetUpViewer()
    {
        if(qv == null)
        {
            var qv_prefab = Resources.Load("QuadViewer") as GameObject;
            var qv_instance = GameObject.Instantiate(qv_prefab);
            qv = qv_instance.GetComponent<QuadViewer>();
        }
        qv.SetQuadTree(this);
    }

    public QuadTreeNode(QuadTreeNode<T> _parent, Vector3 centerPos, Vector2 size, T _data)
    {
        parent = _parent;
#if IS_Y_AXIS_HEIGHT
        centerPos.y = centerPos.z;
        centerPos.z = 0;
#endif  
        if(_parent != null)
        {
            parent = _parent;
            boundary.width = _parent.boundary.width * 0.5f;
            boundary.height = _parent.boundary.width * 0.5f;
            boundary.center = centerPos;
        }
        else
        {
            //centerPos to boundary
            boundary = new Rect();            
            boundary.size = size;
            boundary.center = centerPos;
        }

        data = _data;
        IsSettedData = true;
        SetUpViewer();
        MakeChild();
    }

    //make this for atom children
    public QuadTreeNode(QuadTreeNode<T> _parent, int index)
    {
        const float constantQuadSizeRate = 0.25f;
        parent = _parent;
        boundary.width = _parent.boundary.width * 0.5f;
        boundary.height = _parent.boundary.width * 0.5f;
        var centerPos = Vector2.zero;
        switch(index)
        {
            case 0://left down
                centerPos.x = _parent.boundary.center.x - _parent.boundary.width * constantQuadSizeRate;
                centerPos.y = _parent.boundary.center.y - _parent.boundary.height * constantQuadSizeRate;
                break;
            case 1://right down
                centerPos.x = _parent.boundary.center.x + _parent.boundary.width * constantQuadSizeRate;
                centerPos.y = _parent.boundary.center.y - _parent.boundary.height * constantQuadSizeRate;
                break;
            case 2://left up
                centerPos.x = _parent.boundary.center.x - _parent.boundary.width * constantQuadSizeRate;
                centerPos.y = _parent.boundary.center.y + _parent.boundary.height * constantQuadSizeRate;
                break;
            case 3://right up
                centerPos.x = _parent.boundary.center.x + _parent.boundary.width * constantQuadSizeRate;
                centerPos.y = _parent.boundary.center.y + _parent.boundary.height * constantQuadSizeRate;
                break;
        }
        boundary.center = centerPos;
        SetUpViewer();
    }

public static Vector2 RootSize;
    public static void Init(Vector3 rootCenterPos, Vector2 size, T rootData)
    {
        RootSize = size;
        Root = new QuadTreeNode<T>(null, rootCenterPos, size, rootData);
    }
}
