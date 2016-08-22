using UnityEngine;
using System.Collections;
//using System;

public class NPCSkeletonSwordsman : NPCBrain
{
    public bool allowAttack = true;
	
	protected override IEnumerator Update_AttackState()
	{
		while (true)
		{
			if (Helpers.InRadius(transform.position, target.position, Random.Range(4f, 6f)))
				_desiredMoveSpeed = agent.properties.speed.max * agent.properties.walkToRunSpeedRatio * 0.9f;
			else
				_desiredMoveSpeed = agent.properties.speed.max;

			if (!agent.isStaggered)
			{
				if (target != null)
				{
					if (allowAttack && agent.isGrounded)
					{
						if (Helpers.InRadiusGrounded(transform.position, target.position, 3f))
						{
							if (Random.value > 0.4f)
								_attackController.AttackWithDescriptor(profile.attackCollection.melee1);
							else
								_attackController.AttackWithDescriptor(profile.attackCollection.melee2);
						}
						else
						{
							destination = target.position;
							yield return new WaitForSeconds(0.5f);
						}
					}

					if (_attackController.isAttacking)
					{
						agent.SetDesiredRotation(target);
					}

				}
			}

			yield return null;
		}
	}

	protected override void OnArrivedAtDestination()
    {
        if (target != null)
            destination = target.position;
        base.OnArrivedAtDestination();
    }
}
