using UnityEngine;
using System.Collections;


public class NPCBat : NPCBrain
{

    public Transform target;

    protected override void Update()
    {
        if (target != null)
            destination = target.position;
        base.Update();
    }

}
