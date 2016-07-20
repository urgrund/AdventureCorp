using UnityEngine;
using System.Collections;


public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;
    public bool allowAttack = true;

    protected override void Start()
    {
        // This will auto path find on a coroutine         
        StartCoroutine(TestPathFind());

        //if (target != null)
            //destination = target.position;

        base.Start();
    }
   


    protected override void OnArrivedAtDestination()
    {
        if (target != null)
            destination = target.position;
        base.OnArrivedAtDestination();
    }

    protected override void Update()
    {
		if (Helpers.InRadius(transform.position, target.position, Random.Range(4f, 6f))) 
			_desiredMoveSpeed = agent.properties.speed.max * agent.properties.walkToRunSpeedRatio * 0.9f;
		else
			_desiredMoveSpeed = agent.properties.speed.max;
			

        if (!agent.isStaggered)
        {
            if (target == null)
                return;
            if (allowAttack && agent.isGrounded)
            {
                if (Helpers.InRadiusGrounded(transform.position, target.position, 3f))
                {
					if(Random.value > 0.4f)
						attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);
					else
						attackCollection.controller.AttackWithDescriptor(attackCollection.melee2);
				}
            }
			if (attackCollection.controller.isAttacking)
			{
				agent.SetDesiredRotation(target);
			}
        }
        base.Update();
    }

    IEnumerator TestPathFind()
    {
        yield return new WaitForSeconds(Random.Range(0,2f));

        while (true && !agent.health.isDead)
        {            
            if (target != null)
            {
                destination = target.position;
            }

            yield return new WaitForSeconds(1f);
        }
    }
   
}
