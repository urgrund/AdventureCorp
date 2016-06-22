using UnityEngine;
using System.Collections;

public class EaseTest : MonoBehaviour {

    public float speed = 1f;
    public float dist = 1f;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        	
	}

    void OnDrawGizmos()
    {
        float t = Mathf.Repeat(Time.timeSinceLevelLoad * speed, 1f);

        Vector3 end = transform.position + Vector3.forward * dist;
        Gizmos.DrawLine(transform.position, end);

        t = Easing.Elastic.easeOut(t);       

        Gizmos.DrawWireSphere(transform.position + Vector3.forward * t * dist , 0.5f);

    }
}
