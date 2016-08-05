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

	public float gruntDistanceToRun = 4f;
	public AttackDescriptor gruntJumpAnimation;	


	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if (Random.value > 0.5f)
			return;

		_attackController.YieldControlFromAttack(Random.value > 0.5f);
		agent.SetDesiredRotation(Helpers.DirectionTo(transform, target), true);
		_attackController.AttackWithDescriptor(gruntJumpAnimation);

		base.OnHealthLost(info);
	}


	protected override IEnumerator LogicRoutine()
	{		
		// TODO
		// Make this into a lookup/dictionary at start 
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

		if (Helpers.InRadius(transform, target, gruntDistanceToRun))
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
