using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NPCSkeletonGrunt : NPCBrain
{
	public Transform target;

	protected override void Start()
	{
		state = State.Attack;
		base.Start();
	}

	protected override IEnumerator LogicRoutine()
	{
		//yield return new WaitForSeconds(1f);
		List<AttackDescriptor> aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollection, target);
		if (aDescs != null)
		{
			AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];
			if (_attackController.isAttacking)
			{
				if (_attackController.YieldControlFromAttack())
					_attackController.AttackWithDescriptor(ad, target);
			}
			else
				_attackController.AttackWithDescriptor(ad, target);
		}
		else
			destination = target.position;

		if (Helpers.InRadius(transform, target, 5f))
		{
			_desiredMoveSpeed = agent.properties.speed.max * 0.33f;
			_desiredLookAt = Helpers.DirectionTo(transform, target);
		}
		else
		{
			_desiredLookAt = null;
			_desiredMoveSpeed = agent.properties.speed.max;
		}

		yield return null;
	}
}
