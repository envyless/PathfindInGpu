﻿using System;
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
    public ComputeShader flowMakerComputeShader;        // working in this compute shader 
    Int32 Mainkernel = -1;                              // kernel id
    public Vector3 FlowGoalPos;                         // goal position
    public BufferForGPU.PathInfo[] PathInfos;           // path information length about 10000 over

    public Int32[] ResultIndexes;                         // get result data;

    ComputeBuffer cbPathInfos, cbResultBuffer;
    

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

        // make buffer and set
        cbPathInfos = new ComputeBuffer(PathInfos.Length, PathInfos.GetByteSize<BufferForGPU.PathInfo>());        
        cbPathInfos.SetData(PathInfos);
        flowMakerComputeShader.SetBuffer(Mainkernel, "PathBuffer", cbPathInfos);

        // make buffer and set
        ResultIndexes = new Int32[100];
        cbResultBuffer = new ComputeBuffer(100, ResultIndexes.GetByteSize<Int32>());
        flowMakerComputeShader.SetBuffer(Mainkernel, "ResultIndexes", cbResultBuffer);

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

            Vector3 goalPos = ScreenToWorldPlane.GetWorldPlanePos();

            //set player position
            var naviToGoal = FindObjectOfType<NaviToGoal>();
            var PlayerPosition = naviToGoal.transform.position;
            float x = PlayerPosition.x;
            float y = PlayerPosition.z;

            flowMakerComputeShader.SetFloats("PlayerPosition", x, y);
            flowMakerComputeShader.SetFloats("GoalPosition", goalPos.x, goalPos.z);

            //get
            cbPathInfos.GetData(ResultIndexes);
            
            naviToGoal.PathIndexesToGoal.Clear();
            naviToGoal.PathIndexesToGoal.AddRange(ResultIndexes);
        }
    }

    private void OnDestroy()
    {
        cbPathInfos.Release();
    }
}
