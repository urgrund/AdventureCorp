using UnityEngine;
using System.Collections;

public class Brain : MonoBehaviour
{
    [HideInInspector]
    public Agent agent;
    [HideInInspector]
    public Vector3 currentVelocity;
    [HideInInspector]
    public Quaternion currentRotation;

    float gravitySpeed = 0;

	protected virtual void Update()
    {
        if (!agent.controller.isGrounded)
            gravitySpeed -= agent.gravity;
        else
            gravitySpeed = -agent.controller.stepOffset/Time.deltaTime*5;

        currentVelocity.y = gravitySpeed*Time.deltaTime;
        agent.controller.Move(currentVelocity*Time.deltaTime);
        agent.Rotate(currentRotation);
    }

    public void SetAgent(Agent a)
    {
        agent = a;
    }

    public Quaternion CalculateRotationRelativeToVelocity(float rotationSpeed)
    {
        Vector3 v = currentVelocity;
        v.y = 0;
        return Quaternion.Lerp(agent.transform.rotation, MathLab.CreateRotationToLookAt(v.normalized + agent.transform.position, agent.transform.position), rotationSpeed * Time.deltaTime);
    }

    public Quaternion LookAt(Vector3 target, float rotationSpeed)
    {
        return Quaternion.Lerp(agent.transform.rotation, MathLab.CreateRotationToLookAt((target - agent.transform.position).normalized + agent.transform.position, agent.transform.position), rotationSpeed * Time.deltaTime);
    }

    public void SetVelocity(Vector3 velocity)
    {
        Vector3 v = currentVelocity;
        v.y = 0;
        v = Vector3.MoveTowards(v, velocity, agent.acceleration * Time.deltaTime);
        v = Vector3.ClampMagnitude(v, agent.speed);
        currentVelocity = v;
    }
}
