using UnityEngine;

[System.Serializable]
public class AgentPropertiesMovementContainer
{
    public float max = 5;
    public float acceleration = 30;
    public float damping = 70;
}

public class AgentProperties : MonoBehaviour
{
    [Range(0, 1)]
    public float strafeSpeedFactor = 0.5f;
    [Range(0, 1)]
    public float backPedalSpeedFactor = 0.5f;
    [Range(0, 1)]
    public float walkToRunSpeedRatio = 0.75f;
    public AgentPropertiesMovementContainer speed;
    public AgentPropertiesMovementContainer rotation;

	/// <summary>
	/// Damage to receive before push back counts
	/// </summary>
	public int pushBackDamageThreshold = 1;

	/// <summary>
	/// Scale any pushback influence by this factor
	/// </summary>
	public float pushBackVelocityScale = 1;

	/// <summary>
	/// Rotate toward the damage that killed this agent upon death
	/// </summary>
	public bool rotateOnDeath = false;
}


