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
				yield return new WaitForSeconds(Random.value);
			}
			else
			{
				destination = target.position;
				_desiredMoveSpeed = agent.properties.speed.max;
				yield return new WaitForSeconds((Random.value + 1));
			}
		}
		yield return null;
	}

	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if (info.value > 10)
			agent.Stagger();
		base.OnHealthLost(info);
	}
}
