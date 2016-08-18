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

	protected virtual void Update() { }

    protected virtual void Start()
    {
        _agent.health.onHealthGained += OnHealthGained;
        _agent.health.onHealthLost += OnHealthLost;
        _agent.health.onHealthZero += OnHealthZero;
        _agent.health.onHealthWasInvincible += OnHealthWasInvincible;
		_agent.onStaggered += OnAgentStaggered;
    }	

    // Common delegates that all Brains will be interested in 
    protected virtual void OnHealthLost(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthZero(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthGained(Health.HealthChangedEventInfo info) { }
    protected virtual void OnHealthWasInvincible(Health.HealthChangedEventInfo info) { }
	protected virtual void OnAgentStaggered() { }


	/// <summary>
	/// Class to manage types of debug especially for NPC's
	/// as visualisations may get cluttered in the scene view
	/// </summary>
	public class BrainDebug
	{
		public bool isActive = false;
		public bool showAll = false;
		public bool attack = false;
		public bool agent = false;
		public bool retreat = false;
		public bool awareness = false;
	}
	public BrainDebug debugDraw;
}
