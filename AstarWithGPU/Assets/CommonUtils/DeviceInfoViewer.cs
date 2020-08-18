using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DeviceInfoViewer : MonoBehaviour
{
    public static DeviceInfoViewer Instance;

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

    Rect pos = new Rect(0, 0, Screen.width*0.3f, Screen.height * 0.2f);
    GUIStyle guiStlye = new GUIStyle();
    private void Awake()
    {        
        guiStlye.fontSize = 25;
    }

    // Update is called once per frame
    void OnGUI()
    {
        GUI.Label(pos, "GPU Memory : "+ Profiler.GetAllocatedMemoryForGraphicsDriver() * CommonUtils.Btye2Mb +" Mb", guiStlye);
    }
}
