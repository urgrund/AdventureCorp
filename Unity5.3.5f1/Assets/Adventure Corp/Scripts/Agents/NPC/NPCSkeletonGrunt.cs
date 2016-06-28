using UnityEngine;
using System.Collections;


public class NPCSkeletonGrunt : NPCBrain
{
    public Transform target;

    protected override void Start()
    {
        StartCoroutine(TestPathFind());
        base.Start();
    }

    public AnimationClipProperties animRoar;

    NavMeshPath p;
    IEnumerator TestPathFind()
    {
        yield return new WaitForSeconds(Random.Range(0,2f));
        
        while (true)
        {             
            yield return new WaitForSeconds(1f);
            agent.animatedGameObject.Play(animRoar.clip.name);
        }
    }
    
}
