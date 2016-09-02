using UnityEngine;
using System.Collections;
//using System;

public class NPCSkeletonSwordsman : NPCBrain
{
	//void OnGUI()
	//{
	//	if (GUILayout.Button("Shield UP"))
	//	{
	//		agent.animationController.Play(profile.actions.shieldBlock, true);
	//	}

	//	if (GUILayout.Button("Shield DOWN"))
	//	{
	//		agent.animationController.Stop(profile.actions.shieldBlock, true);
	//	}
	//}


	bool targetAttacked = false;
	void OnTargetAttackStarted(AttackDescriptor d, AttackController c)
	{
		targetAttacked = true;

		bool useShield;
		if (profile.attack.useShieldWhenAttacked.Evaluate())
			useShield = profile.attack.preferShieldOverAvoid.Evaluate();
		else
			useShield = false;

		if (useShield)
		{

		}
		else
		{
			if (profile.attack.jumpAwayToAvoid.Evaluate())
			{
				if (isInsideFarRangeDistance)
				{
					AttackDescriptor ad = null;
					switch (Helpers.RelativeQuadrant(transform, target))
					{
						case Helpers.Quadrant.Back: ad = profile.actions.jumpAway; break;
						case Helpers.Quadrant.Forward: ad = profile.actions.jumpAway; break;
						case Helpers.Quadrant.Left: ad = profile.actions.jumpRight; break;
						case Helpers.Quadrant.Right: ad = profile.actions.jumpLeft; break;
					}

					if (ad != null)
						_attackController.AttackWithDescriptor(ad);
				}
			}
		}
	}


	protected override void Awake()
	{
		if (target != null)
		{
			// Listen to attacks from the player
			if (target.GetComponent<ExplorerBrain>())
				target.GetComponent<AttackController>().onAttackStarted += OnTargetAttackStarted;
		}
		base.Awake();
	}

	protected override IEnumerator Update_AttackState()
	{
		isIgnoreInternalUpdateRoutine = true;

		while (true)
		{
			if (isInsideFarRangeDistance)
			{
				_desiredLookAtTarget = target;
				Vector3 dir = -Helpers.DirectionTo(transform, target) * profile.attack.farRangeDistance;
				destination = transform.position + dir;
				Debug.DrawLine(transform.position, (target.position + dir), Color.cyan, 1f);
			}			

			yield return new WaitForSeconds(1f);
			yield return null;
		}
	}
}
