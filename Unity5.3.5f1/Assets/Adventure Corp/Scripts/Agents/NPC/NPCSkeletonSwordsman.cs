using UnityEngine;
using System.Collections;


public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;

    // Move this to NPCBrain?
    public bool isStaggered = false;
    public float staggerTime = 4f;
    float staggerTimeCount = 0f;

    protected override void Start()
    {
        // This will auto path find on a coroutine         
        StartCoroutine(TestPathFind());

        //if (target != null)
            //destination = target.position;

        base.Start();
    }

    protected override void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        isStaggered = true;
        staggerTimeCount = staggerTime;
        base.OnHealthLost(info);
    }


    protected override void OnArrivedAtDestination()
    {
        if (target != null)
            destination = target.position;
        base.OnArrivedAtDestination();
    }

    protected override void Update()
    {
        if (staggerTimeCount > 0)
        {
            staggerTimeCount -= Time.deltaTime;
            return;
        }

        if (target == null)
            return;

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
