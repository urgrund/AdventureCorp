using UnityEngine;
using System.Collections;

public class TestDamager : MonoBehaviour
{
    public Transform parent;
    public Damage myDamage;

    void Start()
    {
        if (parent != null)
            Helpers.ParentAndCenterOnTransform(transform, parent);
    }

    void OnTriggerEnter(Collider other)
    {
        print("Entered");
        if (other.GetComponent<Health>())
            other.GetComponent<Health>().TakeDamage(myDamage, this.gameObject);
    }

    void OnDrawGizmos()
    {
        if (GetComponent<SphereCollider>())
        {
            SphereCollider c = GetComponent<SphereCollider>();
            Gizmos.color = myDamage.isActive ?  Color.red : Color.grey;
            Gizmos.DrawWireSphere(transform.TransformPoint(c.center), c.radius);
        }
    }
}
