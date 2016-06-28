﻿using UnityEngine;
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
   
}
