﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DeviceInfoViewer : MonoBehaviour
{
    public static DeviceInfoViewer Instance;
    const float byteToMb = 1/(1024f * 1024f);
    [RuntimeInitializeOnLoadMethod]
    public static void SetUp()
    {
        if (Instance == null)
        {
            var go = new GameObject();            
            Instance = go.AddComponent<DeviceInfoViewer>();
            go.name = Instance.name;
        }
    }

    Rect pos = new Rect(0, 0, 300, 30);
    // Update is called once per frame
    void OnGUI()
    {        
        GUI.Label(pos, "GPU Memory : "+ (Profiler.GetAllocatedMemoryForGraphicsDriver()* byteToMb *100f)/100+ " mb");
    }
}
