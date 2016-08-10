using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestZombieBrain : NPCBrain
{
	public Transform target;
	protected override IEnumerator LogicRoutine()
	{
		if (target != null)
		{
			List<AttackDescriptor> aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollection, target);
			if (aDescs != null)
			{
				AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];
				_attackController.AttackWithDescriptor(ad);
			}
			else
			{
				destination = target.position;
				_desiredMoveSpeed = agent.properties.speed.max;
				yield return new WaitForSeconds(1f);
			}
		}
		yield return null;
	}
}
