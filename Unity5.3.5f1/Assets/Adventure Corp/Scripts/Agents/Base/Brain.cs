using UnityEngine;
using System.Collections;

/// <summary>
/// Brain
/// Defines the logic that sends commands to an Agent
/// </summary>
[RequireComponent(typeof(Agent))]
public abstract class Brain : MonoBehaviour
{
    private Agent _agent;
    public Agent agent {  get { return _agent; } }

    protected Vector3 _desiredLookAt = Vector3.zero;
    protected Vector3 _desiredMoveDirection = Vector3.zero;
    protected float _desiredMoveSpeed = 0f;

    protected virtual void Awake()
    {
        _agent = GetComponent<Agent>();
        _agent.PlugBrain(this);
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void MoveAgent()
    {
         _agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, true);                
    }    
}
