using UnityEngine;
using System.Collections;

public class Damager : MonoBehaviour
{
    public Transform parent;
    public Damage damage;    

    void Start()
    {
        if (parent != null)
            Helpers.ParentAndCenterOnTransform(transform, parent);
    }

    void OnTriggerEnter(Collider other)
    {
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
