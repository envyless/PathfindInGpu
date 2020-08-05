using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonUtilsExtension 
{

    public unsafe static int GetSize<T>() where T : unmanaged
    {
        return sizeof(T);
    }

    public static int GetByteSize<T>(this List<T> list) where T : unmanaged
    {
        return GetSize<T>() * list.Count;
    }

    public static int GetByteSize<T>(this Array list) where T : unmanaged
    {
        return GetSize<T>() * list.Length;
    }
}
