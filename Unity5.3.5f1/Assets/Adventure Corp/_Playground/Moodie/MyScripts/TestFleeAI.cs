using UnityEngine;
using System.Collections;

public class TestFleeAI : NPCBrain
{
    bool _isPatrol = true;
    bool _isRetreat = false;
    protected void CheckPatrolOrRetreat()
    {
        if (!_isRetreat)
        {
            if (Vector3.Distance(LevelManager.players[0].transform.position, transform.position) < 6)
            {

                _isRetreat = true;
                _isPatrol = false;
            }
        }
        else if (_isRetreat)
        {
            if (Vector3.Distance(LevelManager.players[0].transform.position, transform.position) > 20)
            {
                _isPatrol = true;
                _isRetreat = false;
            }
        }
    }

    protected override IEnumerator Update_RetreatState()
    {
        while (!_isRetreat)
            yield return null;

        while (_isRetreat)
        {
			CheckPatrolOrRetreat();
            destination = FindRetreatTarget(); // Find a retreat position to move towards to

            float t = 0;
            while (t < 0.2f && _isRetreat) // If I am still retreating, wait X time before calculating a new retreat point
            {
                t += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }
        yield return null;
    }

    protected override IEnumerator Update_PatrolState()
    {
        while (!_isPatrol)
            yield return null;

        while (_isPatrol)
        {
			CheckPatrolOrRetreat();
			if (PatrolManager.instance)
                patrolProperties = PatrolManager.instance.GrabPatrolProperties(this); // Find the nearest patrol zone and point

            if (!patrolProperties.patrolPoint) // If patrol point does not exist just stand still
                destination = transform.position;
            else
                destination = patrolProperties.patrolPoint.transform.position; // Move towards patrol point

            while (_navMeshNextPosition != null && _isPatrol) // Until destination is reached and I am still patroling keep moving towards destination
                yield return null;

            float t = 0;
            while (t < 2.5f && _isPatrol) // Once arrive to destination and I am still patroling, wait X time on the patrol point
            {
                t += Time.deltaTime;
                yield return null;
            }

            yield return null;
        }

        patrolProperties.Clear();
        yield return null;
    }
}