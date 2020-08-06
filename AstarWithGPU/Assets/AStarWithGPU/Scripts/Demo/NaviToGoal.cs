using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NaviToGoal : MonoBehaviour
{
    /// <summary>
    /// move this character
    /// </summary>
    public Vector3 GoalPos;
    public float Speed = 10;
    public Vector3 MoveDir;

    private void Update()
    {       
        MoveDir = FlowMaker.Instance.GetFlow(transform.position);
        if (MoveDir != Vector3.zero)
        {
            GoalPos = ScreenToWorldPlane.GetWorldPlanePos();
        }

        MoveDir = GoalPos - transform.position;
        transform.position += (MoveDir.normalized * Time.deltaTime* Speed);

        


    }    
}
