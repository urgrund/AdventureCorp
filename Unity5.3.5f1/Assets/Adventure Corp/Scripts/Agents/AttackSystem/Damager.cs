﻿using UnityEngine;
using System.Collections;


public class Damager : MonoBehaviour
{
    //public Transform parent;
    public Damage damage;
    public SphereCollider sphereCollider; 

    
    void OnTriggerEnter(Collider other)
    {
        if (!enabled)
            return;

        if (damage == null)
            return;

        if (other.GetComponent<Health>())
            other.GetComponent<Health>().TakeDamage(damage, this.gameObject);
    }


    void OnDrawGizmos()
    {
        // TODO - we will need other shapes :) 
        if (GetComponent<SphereCollider>())
        {
            if (damage == null)
                return;

            SphereCollider c = GetComponent<SphereCollider>();
            Gizmos.color = enabled ? Color.red : Color.grey;
            Gizmos.DrawWireSphere(transform.TransformPoint(c.center), c.radius);            
        }
    }    
}
