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
		if (info.responsibleAttackController == null)
			return; 

		if (Random.value > 0.33f)
			return;

		_attackController.YieldControlFromAttack(Random.value > 0.5f);
		agent.SetDesiredRotation(Helpers.DirectionTo(transform, target), true);
		_attackController.AttackWithDescriptor(gruntJumpAnimation);

		base.OnHealthLost(info);
	}



	void Attack(AttackDescriptor ad)
	{
		if (_attackController.isAttacking)
		{
			if (_attackController.YieldControlFromAttack())
				_attackController.AttackWithDescriptor(ad, target);
		}
		else
			_attackController.AttackWithDescriptor(ad, target);
	}

	

	protected override IEnumerator LogicRoutine()
	{		
		// TODO
		// Make this into a lookup/dictionary at start 
		List<AttackDescriptor> aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollection, target);
		if (aDescs != null)
		{
			AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];

			// If the distance of the attack is 'far', then 
			// chose whether to actually use it or to run
			// closer for a different attack
			if (!Helpers.InRadius(transform, target, gruntDistanceToRun))
			{
				if (Random.value > 0.2f)
				{
					_desiredMoveSpeed = agent.properties.speed.max;
					_desiredLookAtTarget = target;
					yield return new WaitForSeconds(0.3f);												
				}
				else
					Attack(ad);
			}
			else
				Attack(ad);
		}
		else
			destination = target.position;


		// Change speed depending on distance 
		if (Helpers.InRadius(transform, target, gruntDistanceToRun))
		{
			_desiredMoveSpeed = agent.properties.speed.max * 0.33f;
			_desiredLookAtTarget = target;
		}
		else
		{
			_desiredLookAt = null;
			_desiredMoveSpeed = agent.properties.speed.max;
		}

		yield return null;
	}
}
