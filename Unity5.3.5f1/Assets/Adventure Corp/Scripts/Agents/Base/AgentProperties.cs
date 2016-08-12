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
    public float walkToRunSpeedRatio = 0.25f;
    public AgentPropertiesMovementContainer speed;
    public AgentPropertiesMovementContainer rotation;
    public int pushBackDamageThreshold = 1;
	public float pushBackVelocityScale = 1;	
}


