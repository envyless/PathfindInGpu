using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    public static Ground Instance = null;

    [RuntimeInitializeOnLoadMethod]
    public static void SetUp()
    {
        if(Instance == null)
        {
            var ground = GameObject.Find("Ground");
            if(!ground)
            {
                ground = new GameObject("Ground");
            }
            Instance = ground.AddComponent<Ground>();
        }
    }
}
