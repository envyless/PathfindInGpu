using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawerMgr : MonoBehaviour
{
    public static LineDrawerMgr Instance;

    public List<LineDrawer> listLineDrawer = new List<LineDrawer>();

    [RuntimeInitializeOnLoadMethod]
    public static void SetUp()
    {
        if(Instance == null)
        {
            var go = new GameObject();
            go.name = "LineDrawerMgr";
            Instance = go.AddComponent<LineDrawerMgr>();
        }
    }

    public static void DrawLine(Vector3 position1, Vector3 position2, Color? color = null, float consistTime = 0)
    {
        if (Instance == null)
            return;
        var ld = new LineDrawer();

        Color defualtColor = Color.white;
        if(color.HasValue)
            defualtColor = color.Value;
        
        ld.DrawLineInGameView(position1, position2, defualtColor);
        ld.SetTime(consistTime);
        Instance.listLineDrawer.Add(ld);
    }

    private void Update()
    {
        for(int i = 0; i < listLineDrawer.Count; ++i)
        {
            if (listLineDrawer[i].IsFinished(Time.deltaTime))
            {
                listLineDrawer[i].Destroy();
                listLineDrawer.RemoveAt(i);
            }
        }
    }
}
