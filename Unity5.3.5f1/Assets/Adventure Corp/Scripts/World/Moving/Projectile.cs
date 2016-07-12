using UnityEngine;
using System.Collections;


public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public float lifeTime = 10f;
    
    void Start()
    {
        Destroy(this.gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }    
}
