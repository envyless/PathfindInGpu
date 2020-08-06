using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Flow Maker that make flow with Path Informations
/// when some many characters move to goal that need calcuate each ai, but this use flow buffer.
/// 
/// 1. Make Path Info buffer using brushfire Algorithm from goal
/// 2. and then every character use that buffer. select lowest cost 
/// 
/// so here is following
/// 
/// - path info buffer - contains cost S(from start) + cost G(from goal)
/// - ai buffer - contains index of AI position on path
/// 
/// - ai will call GetDirection() if ai didn't get goal from Result
/// </summary
public class FlowMaker : MonoBehaviour
{
    public static FlowMaker Instance;    
    ObstacleMaker obstacleMaker;

    //shader vars
    public ComputeShader flowMakerComputeShader;
    Int32 Mainkernel = -1;    
    public Vector3 FlowGoalPos;
    public BufferForGPU.PathInfo[] PathInfos;

    ComputeBuffer cbPathInfos;
    

    private void Awake()
    {
        Instance = FindObjectOfType<FlowMaker>();
        obstacleMaker = FindObjectOfType<ObstacleMaker>();

        PathInfos = BufferForGPU.MakePathInfos(obstacleMaker.NumObstacleW, obstacleMaker.NumObstacleH, Vector3.zero);
    }

    private void Start()
    {
        SetUpShader();
    }

    private void SetUpShader()
    {        
        Mainkernel = flowMakerComputeShader.FindKernel("CSMain");
        cbPathInfos = new ComputeBuffer(PathInfos.Length, PathInfos.GetByteSize<BufferForGPU.PathInfo>());
        cbPathInfos.SetData(PathInfos);
        flowMakerComputeShader.SetBuffer(Mainkernel, "PathBuffer", cbPathInfos);
        flowMakerComputeShader.SetInt("NumWidth", obstacleMaker.NumObstacleW);
    }    

    public Vector3 GetFlow(Vector3 position)
    {
        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.Mouse1))
            dir = ScreenToWorldPlane.GetWorldPlanePos() - position;

        return dir;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.LogError("mouse Down 1");
            flowMakerComputeShader.Dispatch(Mainkernel, 1, 1, 1);

            //get
            cbPathInfos.GetData(PathInfos);
            Debug.LogError(PathInfos[0].Position);
        }
    }

    private void OnDestroy()
    {
        cbPathInfos.Release();
    }
}
