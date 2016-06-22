using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Agent : MonoBehaviour
{
    [HideInInspector]
    public Brain brain;
    [HideInInspector]
    public CharacterController controller;

    public float speed = 5;
    public float acceleration = 30;
    public float rotationSpeed = 20;
    public float gravity = 10;

    void Awake ()
    {
        brain = GetComponent<Brain>();
        controller = GetComponent<CharacterController>();

        if (brain)
            PlugBrain(brain);
        else
            Debug.LogError("There is no Brain attached on me, how am I supposed to function?");
    }
	
	public void PlugBrain(Brain b)
    {
        brain = b;
        b.SetAgent(this);
    }

    public void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
}
