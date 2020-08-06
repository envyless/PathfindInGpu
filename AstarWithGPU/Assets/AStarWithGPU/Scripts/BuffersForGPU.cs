using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferForGPU
{
    public static int NumOfWidth, NumOfHeight;
    public static Vector2 CenterPosition, StartPosition;
    public static int NumMax => NumOfHeight* NumOfWidth;
    public static PathInfo[] MakePathInfos(int numWidth, int numHeight, Vector2 centerPos)
    {
        NumOfWidth = numWidth;
        NumOfHeight = numHeight;
        CenterPosition = centerPos;
        StartPosition = CenterPosition - new Vector2(NumOfWidth, NumOfHeight) * 0.5f;

        PathInfo[] pathInfos = new PathInfo[numWidth * NumOfHeight];
        return pathInfos;
    }

    public static int CalcuateIndex(int w, int h)
    {
        Vector2 position = new Vector2(w, h);
        var relativePos = position - StartPosition;
        var Index = -1;

        if (relativePos.x < 0 || relativePos.y < 0 || relativePos.x >= NumOfWidth || relativePos.y >= NumOfHeight)
        {
            //out of index
            return Index;
        }

        Index = (int)relativePos.x + (int)relativePos.y * NumOfWidth;
        return Index;
    }

    /// <summary>
    /// path information is in each cell of map
    /// </summary>
    public struct PathInfo
    {
        private byte _value;
        public bool IsNotPathAble {
            get
            {
                return _value == 0;
            }
            set
            {
                _value = value ? (byte)1 : (byte)0;               
            }
        }
        public int Index;

        //position
        public Vector2 Position;

        public int CalcuateIndex(Vector2 position)
        {
            Position.x = (int)position.x;
            Position.y = (int)position.y;            
            Index = BufferForGPU.CalcuateIndex((int)Position.x, (int)position.y);
            
            return Index;
        }
    }

}