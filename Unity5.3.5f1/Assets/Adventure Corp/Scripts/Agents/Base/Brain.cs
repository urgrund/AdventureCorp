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
    public Agent agent {  get { return _agent; } set { _agent = value; } }

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
}
