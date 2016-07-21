using UnityEngine;
using System.Collections;
//using System;

public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;
    public bool allowAttack = true;
	
	protected override IEnumerator LogicRoutine()
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
							attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);
						else
							attackCollection.controller.AttackWithDescriptor(attackCollection.melee2);
					}
				}
				else if (attackCollection.controller.isAttacking)
				{
					agent.SetDesiredRotation(target);
				}
				else
				{
					destination = target.position;
					yield return new WaitForSeconds(1f);
				}
			}
		}

		yield return null;
	}

	protected override void OnArrivedAtDestination()
    {
        if (target != null)
            destination = target.position;
        base.OnArrivedAtDestination();
    }

  //  protected override void Update()
  //  {
		//if (Helpers.InRadius(transform.position, target.position, Random.Range(4f, 6f))) 
		//	_desiredMoveSpeed = agent.properties.speed.max * agent.properties.walkToRunSpeedRatio * 0.9f;
		//else
		//	_desiredMoveSpeed = agent.properties.speed.max;
			

  //      if (!agent.isStaggered)
  //      {
  //          if (target == null)
  //              return;
  //          if (allowAttack && agent.isGrounded)
  //          {
  //              if (Helpers.InRadiusGrounded(transform.position, target.position, 3f))
  //              {
		//			if(Random.value > 0.4f)
		//				attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);
		//			else
		//				attackCollection.controller.AttackWithDescriptor(attackCollection.melee2);
		//		}
  //          }
		//	if (attackCollection.controller.isAttacking)
		//	{
		//		agent.SetDesiredRotation(target);
		//	}
  //      }
  //      base.Update();
  //  }
}
