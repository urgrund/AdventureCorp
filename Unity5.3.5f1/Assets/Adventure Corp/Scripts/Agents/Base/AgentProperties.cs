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
    [Range(0, 10)]
    public float gravity = 10;
    [Range(0, 1)]
    public float strafeSpeedFactor = 0.5f;
    public AgentPropertiesMovementContainer speed;
    public AgentPropertiesMovementContainer rotation;
}


