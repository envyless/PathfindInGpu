using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMaker : MonoBehaviour
{
    private Camera gameCamera;
    private GameObject goObstacles;

    private bool IsMakeState = false;

    public BufferForGPU.PathInfo[] PathInfos;

    public int NumObstacleW = 100, NumObstacleH = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        gameCamera = Camera.main;
        goObstacles = GameObject.Find("Obstacles");
        PathInfos = FlowMaker.Instance.PathInfos;                
    }

    //fixed is called more frequently
    void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            IsMakeState = true;            
        }        
        else if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            IsMakeState = false;
        }

        if(IsMakeState)
        {
            var worldPos = ScreenToWorldPlane.GetWorldPlanePos();

            //try 4 times for make more big wall
            MakeObstacleInIndex(worldPos);
            MakeObstacleInIndex(worldPos + Vector3.right);
            MakeObstacleInIndex(worldPos + Vector3.forward);
            MakeObstacleInIndex(worldPos + Vector3.right + Vector3.forward);
        }
    }

    private void MakeObstacleInIndex(Vector3 worldPos)
    {
        //get index to check already made
        int index = BufferForGPU.CalcuateIndex((int)worldPos.x, (int)worldPos.z);

        Debug.LogError("index : " + index + "\nWorld : " + worldPos);
        if (index >= PathInfos.Length || index < 0)
            return;

        //when you can go there, we can make obstacle in there
        var is_make_able = !PathInfos[index].IsNotPathAble;
        if (is_make_able)
        {
            var originObstacle = Resources.Load<GameObject>("Obstacle");
            var instancedObstacle = GameObject.Instantiate(originObstacle, goObstacles.transform);
            worldPos.y = 1.5f;

            instancedObstacle.transform.position = worldPos;
            PathInfos[index].IsNotPathAble = true;
            Debug.LogError(worldPos);
        }
    }
}
