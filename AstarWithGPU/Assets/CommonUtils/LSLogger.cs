 public static class LSLogger
 {
     [Conditional("QA"), Conditional("TEST"), Conditional("UNITY_EDITOR")]
     public static void Log(string logString, Color? color = null)
     {
         if (color == null)
             color = Color.white;
         
-        Debug.Log("<color=#"+ColorUtility.ToHtmlStringRGB(color.Value)+">"+logString+"</color>");
+        Debug.Log("<color=#"+ColorUtility.ToHtmlStringRGB(color.Value)+"> "+logString+" </color>");
     }
 
     public static void LogError(string logString)
     {
         Debug.LogError(logString);
     }
 }
