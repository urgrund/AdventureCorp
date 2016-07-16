using UnityEngine;
using System.Collections;


public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;

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
        if (Helpers.InRadiusGrounded(transform.position, target.position, 3.5f))
        {
            attackController.AttackWithDescriptor(attackController.attacks[2]);
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
