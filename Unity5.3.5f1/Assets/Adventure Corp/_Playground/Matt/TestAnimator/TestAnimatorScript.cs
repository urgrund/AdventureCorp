using UnityEngine;
using System.Collections;

public class TestAnimatorScript : MonoBehaviour {

    public Animator animator;

    
	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("Fire", Input.GetKeyDown(KeyCode.Space));
        animator.SetBool("Run", Input.GetKey(KeyCode.UpArrow));

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            animator.Play("attack");
        
                
    }
}
