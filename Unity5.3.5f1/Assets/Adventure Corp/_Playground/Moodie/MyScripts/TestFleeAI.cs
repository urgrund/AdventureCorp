using UnityEngine;
using System.Collections;

public class TestFleeAI : NPCBrain
{
    protected override IEnumerator LogicRoutine()
    {
        if (state != State.Patrol)
            yield return null;


        if(state == State.Patrol || state == State.Idle)
        {
            if (Vector3.Distance(LevelManager.players[0].transform.position, transform.position) < 6 && state != State.Retreat)
                state = State.Retreat;
            else if(state != State.Patrol)
                state = State.Patrol;
        }
        else if(state == State.Retreat)
        {
            if (Vector3.Distance(LevelManager.players[0].transform.position, transform.position) > 20)
                state = State.Patrol;
        }
       
        yield return null;
    }
}
