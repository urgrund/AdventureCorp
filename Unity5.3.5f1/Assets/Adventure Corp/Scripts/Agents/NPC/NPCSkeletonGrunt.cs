using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NPCSkeletonGrunt : NPCBrain
{
	//public float gruntDistanceToRun = 4f;
	//public AttackDescriptor gruntJumpAnimation;	

	//protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	//{
	//	if (info.responsibleAttackController == null)
	//		return; 

	//	if (Random.value > 0.33f)
	//		return;

	//	_attackController.YieldControlFromAttack(Random.value > 0.5f);
	//	agent.SetDesiredRotation(Helpers.DirectionTo(transform, target), true);
	//	_attackController.AttackWithDescriptor(gruntJumpAnimation);

	//	base.OnHealthLost(info);
	//}


	//float lastAtkTime = 0f;
	//IEnumerator Attack(AttackDescriptor ad)
	//{
	//	yield return new WaitForEndOfFrame();
	//	if (_attackController.isPastYieldControlTime)
	//	{
	//		_attackController.AttackWithDescriptor(ad, target);		
	//		lastAtkTime = Time.time;
	//		hasWaited = false;
	//		yield return new WaitForEndOfFrame();			
	//	}
	//}


	//protected override IEnumerator Update_AttackState()
	//{
	//	isIgnoreInternalUpdateRoutine = true;
	//	while (true)
	//	{
	//		_attackController.AttackWithDescriptor(profile.attackCollection.melee2);
	//		yield return null;			
	//	}
	//}




	bool hasWaited = false;
	//protected override IEnumerator Update_AttackState()
	//{
	//	while (true)
	//	{
	//		// TODO
	//		// Make this into a lookup/dictionary at start 
	//		List<AttackDescriptor> aDescs = _attackController.GetSuggestedAttacksForTarget(profile.attackCollection, target);
	//		if (aDescs != null)
	//		{
	//			AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];

	//			// If the distance of the attack is 'far', then 
	//			// chose whether to actually use it or to run
	//			// closer for a different attack
	//			if (!Helpers.InRadius(transform, target, gruntDistanceToRun))
	//			{
	//				if (Random.value > 0.5f)
	//				{
	//					_desiredMoveSpeed = agent.properties.speed.max;
	//					_desiredLookAtTarget = target;
	//					if (!hasWaited)
	//					{
	//						yield return new WaitForSeconds(0.25f);
	//						hasWaited = true;
	//					}
	//				}
	//				else
	//					yield return Attack(ad);
	//			}
	//			else
	//				yield return Attack(ad);
	//		}
	//		else
	//			destination = target.position;


	//		// Change speed depending on distance 
	//		if (Helpers.InRadius(transform, target, gruntDistanceToRun))
	//		{
	//			_desiredMoveSpeed = agent.properties.speed.max * 0.33f;
	//			_desiredLookAtTarget = target;
	//		}
	//		else
	//		{
	//			_desiredLookAt = null;
	//			_desiredMoveSpeed = agent.properties.speed.max;
	//		}

	//		yield return null;
	//	}
	//}
}
