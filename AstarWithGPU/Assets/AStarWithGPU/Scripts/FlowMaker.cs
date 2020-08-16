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
    public ComputeShader flowMakerComputeShader;        // working in this compute shader 
    Int32 Mainkernel = -1;                              // kernel id
    public Vector3 FlowGoalPos;                         // goal position
    public BufferForGPU.PathInfo[] PathInfos;           // path information length about 10000 over

    public BufferForGPU.CalculatePathInfo[] ResultIndexes;                         // get result data;

    ComputeBuffer cbPathInfos, cbResultBuffer;
    

    private void Awake()
    {
        Instance = FindObjectOfType<FlowMaker>();
        obstacleMaker = FindObjectOfType<ObstacleMaker>();

        PathInfos = BufferForGPU.MakePathInfos(obstacleMaker.NumObstacleW, obstacleMaker.NumObstacleH, Vector3.zero);

        for (int i = 0; i < PathInfos.Length; i++)
        {
            PathInfos[i].Index = i;
            PathInfos[i].CalcPos();
        }

        Debug.LogError(PathInfos[0].Position);
    }

    private void Start()
    {
        SetUpShader();
        ComputeBufferToTexture.SetScaleTexture(3);
    }

    private void SetUpShader()
    {        
        Mainkernel = flowMakerComputeShader.FindKernel("CSMain");

        // make buffer and set
        cbPathInfos = new ComputeBuffer(PathInfos.Length, CommonUtilsExtension.GetByteSize<BufferForGPU.PathInfo>());
        cbPathInfos.SetData(PathInfos);
        flowMakerComputeShader.SetBuffer(Mainkernel, "PathBuffer", cbPathInfos);

        // make buffer and set
        ResultIndexes = new BufferForGPU.CalculatePathInfo[10000];
        cbResultBuffer = new ComputeBuffer(10000, CommonUtilsExtension.GetByteSize<BufferForGPU.CalculatePathInfo>());
        flowMakerComputeShader.SetBuffer(Mainkernel, "CalcPathBuffer", cbResultBuffer);

        flowMakerComputeShader.SetInt("NumWidth", obstacleMaker.NumObstacleW);
        flowMakerComputeShader.SetInt("NumHeight", obstacleMaker.NumObstacleH);
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
            cbPathInfos.SetData(PathInfos);            
            flowMakerComputeShader.SetBuffer(Mainkernel, "PathBuffer", cbPathInfos);
            Vector3 goalPos = ScreenToWorldPlane.GetWorldPlanePos();

            //set player position
            var naviToGoal = FindObjectOfType<NaviToGoal>();
            var PlayerPosition = naviToGoal.transform.position;
            float x = PlayerPosition.x;
            float y = PlayerPosition.z;

            var playerIndex = BufferForGPU.CalcuateIndex((int)PlayerPosition.x, (int)PlayerPosition.z);
            var goalIndex = BufferForGPU.CalcuateIndex((int)goalPos.x, (int)goalPos.z);
            flowMakerComputeShader.SetInt("PlayerIndex", playerIndex);
            flowMakerComputeShader.SetInt("GoalIndex", goalIndex);
            flowMakerComputeShader.SetFloats("PlayerPosition", x, y);
            flowMakerComputeShader.SetFloats("GoalPosition", goalPos.x, goalPos.z);            
            flowMakerComputeShader.Dispatch(Mainkernel, 1, 1, 1);

            Debug.LogError("goal index : " + goalIndex);
            cbResultBuffer.GetData(ResultIndexes);
            cbPathInfos.GetData(PathInfos);
            //get
            /*
            
            naviToGoal.PathIndexesToGoal.Clear();
            naviToGoal.PathIndexesToGoal.AddRange(ResultIndexes);*/

            ComputeBufferToTexture.Instance.SetTexture(
                "PathInfos",
                PathInfos, 
                (index=>{
                    return new Color(PathInfos[index].IsNotPathAble == true ? 1 : 0, 0, 0); }) // color setting                
                );

            ComputeBufferToTexture.Instance.SetTexture(
                "ResultIndexes",
                ResultIndexes,
                (index => { return new Color(ResultIndexes[index].Cost, 0, 0); }) // color setting                
                );

            ComputeBufferToTexture.SetText(
                ResultIndexes, (index =>
                {
                    if(ResultIndexes[index].BaseIndex == 0)
                    {
                        return (string.Empty, Vector3.zero);
                    }
                   
                    return (ResultIndexes[index].BaseIndex.ToString(), new Vector3(PathInfos[index].Position.x, 0, PathInfos[index].Position.y));
                })
                );
        }
    }

    private void OnDestroy()
    {
        cbPathInfos?.Release();
        cbResultBuffer?.Release();
    }
}
