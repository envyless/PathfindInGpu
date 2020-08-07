using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class StopwatchUtil
{
    private static StopwatchUtil _instance;
    
    [Conditional("UNITY_EDITOR")]
    [RuntimeInitializeOnLoadMethod]
    private static void SetUp()
    {
        _instance = new StopwatchUtil();        
    }
    
    private Dictionary<string, Stopwatch> _stopwatches = new Dictionary<string, Stopwatch>();

    [Conditional("UNITY_EDITOR")]
    public static void Start(string key = "")
    {
        if (!_instance._stopwatches.TryGetValue(key, out var sw))
        {
            sw = new Stopwatch();
            _instance._stopwatches.Add(key, sw);
        }               
        sw.Start();
    }
    
    [Conditional("UNITY_EDITOR")]
    public static void Stop(string key = "")
    {
        if (!_instance._stopwatches.TryGetValue(key, out var sw))
        {
            sw = new Stopwatch();    
            _instance._stopwatches.Add(key, sw);
        }               
        sw.Stop();                
        LSLogger.Log("sw.ElapsedMilliseconds : "+sw.ElapsedMilliseconds);                
    }
}
