using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<int> PathIndexesToGoal = new List<int>();

    enum State
    {
        Stop,
        Moving,        
    }

    State myState;

    private void Update()
    {
        switch (myState)
        {
            case State.Stop:
                if (PathIndexesToGoal.Count > 0)
                {                    
                    var index = PathIndexesToGoal.Count - 1;
                    var targetPathIndex = PathIndexesToGoal[index];

                    var nextPathInfo = FlowMaker.Instance.PathInfos[targetPathIndex];
                    GoalPos = nextPathInfo.Position;
                    GoalPos.z = GoalPos.y;
                    GoalPos.y = 0;
                   
                    PathIndexesToGoal.RemoveAt(index);
                    myState = State.Moving;
                }
                break;
            case State.Moving:
                var ToGoal = GoalPos - transform.position;
                transform.position += (ToGoal.normalized * Time.deltaTime * Speed);

                if(ToGoal.magnitude < 1)
                {
                    myState = State.Stop;
                }
                break;
        }

        
    }
}
