using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NavAgent : MonoBehaviour
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
        if(Input.GetKeyDown(KeyCode.Space))
        {
            myState = State.Moving;
            GoalPos = ScreenToWorldPlane.GetWorldPlanePos();
            GoalPos.y = transform.position.y;
        }

        //show where i am?
        for (int i = 0; i < MeshToQuadtree.indices.Length - 2; i += 3)
        {
            var i0 = MeshToQuadtree.indices[i];
            var i1 = MeshToQuadtree.indices[i + 1];
            var i2 = MeshToQuadtree.indices[i + 2];

            var v0 = MeshToQuadtree.vertices[i0];
            var v1 = MeshToQuadtree.vertices[i1];
            var v2 = MeshToQuadtree.vertices[i2];

            if(CommonUtils.TriangleMethod.IsContain(v0, v1, v2, transform.position))
                CommonUtils.DrawTriangle(v0, v1, v2, Color.red, 1, 1.2f);            
        }

        //state machine
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

                if (ToGoal.magnitude < 1)
                {
                    myState = State.Stop;
                }
                break;
        }
    }
}
