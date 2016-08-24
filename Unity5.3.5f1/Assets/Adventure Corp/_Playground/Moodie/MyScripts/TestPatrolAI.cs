using UnityEngine;
using System.Collections;

public class TestPatrolAI : NPCBrain
{

	protected override void Start()
	{
		state = State.Patrol;
		base.Start();
	}

	//protected override IEnumerator Update_PatrolState()
 //   {
 //       while (true)
 //       {
 //           if (PatrolManager.instance)
 //               patrolProperties = PatrolManager.instance.GrabPatrolProperties(this); // Find the nearest patrol zone and point

 //           if (!patrolProperties.patrolPoint) // If patrol point does not exist just stand still
 //               destination = transform.position;
 //           else
 //               destination = patrolProperties.patrolPoint.transform.position; // Move towards patrol point

 //           while (_navMeshNextPosition != null) // Until destination is reached and I am still patroling keep moving towards destination
 //               yield return null;

 //           float t = 0;
 //           while (t < 2.5f) // Once arrive to destination and I am still patroling, wait X time on the patrol point
 //           {
 //               t += Time.deltaTime;
 //               yield return null;
 //           }

 //           yield return null;
 //       }
 //   }
}