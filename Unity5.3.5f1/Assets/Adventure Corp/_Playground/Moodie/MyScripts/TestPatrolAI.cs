using UnityEngine;
using System.Collections;

public class TestPatrolAI : NPCBrain
{
	protected override IEnumerator LogicRoutine()
	{
		if (state != State.Patrol)
			yield return null;

		state = State.Patrol;
		yield return null;
	}
}
