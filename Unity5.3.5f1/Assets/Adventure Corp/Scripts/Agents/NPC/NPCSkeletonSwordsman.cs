using UnityEngine;
using System.Collections;


public class NPCSkeletonSwordsman : NPCBrain
{
    public Transform target;

    protected override void Start()
    {
        StartCoroutine(TestPathFind());
        base.Start();
    }   


    NavMeshPath p;
    IEnumerator TestPathFind()
    {
        yield return new WaitForSeconds(Random.Range(0,2f));
        
        while (true)
        {            
            if (target != null)
            {
                destination = target.position;          
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void OnDrawGizmos()
    {
        p = _navMeshPathToDestination;
        if (p == null)
            return;

        Gizmos.color = Color.red;
        for (int i=0; i<p.corners.Length; i++)
        {
            if ((i + 1) < p.corners.Length)
                Gizmos.DrawLine(p.corners[i], p.corners[i + 1]);
            Gizmos.DrawWireSphere(p.corners[i], 0.25f);
        }
    }
}
