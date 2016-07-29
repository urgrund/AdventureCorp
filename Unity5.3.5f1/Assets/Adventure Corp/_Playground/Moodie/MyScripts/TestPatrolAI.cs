using UnityEngine;
using System.Collections;

public class TestPatrolAI : NPCBrain
{
	protected override IEnumerator LogicRoutine()
	{
		if (isPatrol)
			yield return null;

		isPatrol = true;
		yield return null;
	}
}
