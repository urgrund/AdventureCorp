using UnityEngine;
using System.Collections;


public class Damager : MonoBehaviour
{
    /// <summary>
    /// Damage properties for this Damager
    /// </summary>
    public Damage damage;

    /// <summary>
    /// Sphere collider defining the Damager zone
    /// </summary>
    public SphereCollider sphereCollider;

    /// <summary>
    /// If this is set, this Health will not take damage from this Damager
    /// </summary>
    public Health owner;

    /// <summary>
    /// Delay the activation of this Damager
    /// </summary>
    public bool isDelayEnable = false;
    public float initialActivateDelay = 0f;

    void Awake()
    {
        if (isDelayEnable)
        {
            StartCoroutine(DelayEnable());
            enabled = false;
        }
    }
        

    IEnumerator DelayEnable()
    {
        enabled = false;
        yield return new WaitForSeconds(initialActivateDelay);
        enabled = true;
    }

    
    void OnTriggerEnter(Collider other)
    {
        if (!enabled)
            return;

        if (damage == null)
            return;
            
        Health g = other.GetComponent<Health>();
        if (g !=null )
            if(g != owner)
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
