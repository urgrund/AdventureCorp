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
    public Agent agent { get { return _agent; } }

    protected virtual void Awake()
    {
        _agent = GetComponent<Agent>();
    }

    protected virtual void Start()
    {
        _agent.health.onHealthGained += OnHealthGained;
        _agent.health.onHealthLost += OnHealthLost;
        _agent.health.onHealthZero += OnHealthZero;
        _agent.health.onHealthWasInvincible += OnHealthWasInvincible;
    }

    protected virtual void Update() { }

    // Common delegates that all Brains will be interested in 
    protected virtual void OnHealthLost(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthZero(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthGained(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthWasInvincible(Health.HealthChangedEventInfo info) { }
}
