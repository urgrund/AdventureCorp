using UnityEngine;
using System.Collections;


public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;

    protected override void Start()
    {
        _desiredMoveSpeed = agent.properties.speed.max;

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


    IEnumerator TestPathFind()
    {
        yield return new WaitForSeconds(Random.Range(0,2f));

        

        while (true)
        {            
            if (target != null)
            {
                destination = target.position;
                //_desiredMoveSpeed = agent.properties.speed.max * Random.Range(0.3f, 1.0f);
            }
            yield return new WaitForSeconds(2f);
        }
    }
   
}
