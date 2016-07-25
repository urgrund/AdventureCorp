using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NPCSkeletonGrunt : NPCBrain
{
	public Transform target;

	protected override IEnumerator LogicRoutine()
	{
		print("foo!!!");
		yield return new WaitForSeconds(1f);
		List<AttackDescriptor> ad = _attackController.GetSuggestedAttacksForTarget(attackCollection, target);
		if (ad != null)
		{
			foreach (AttackDescriptor a in ad)
			{
				print(a.name + "   was suggested.");
			}
		}
		yield return null;
	}
}
