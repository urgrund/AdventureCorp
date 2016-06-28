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
    public float gravity = 10;    
    public AgentPropertiesMovementContainer speed;
    public AgentPropertiesMovementContainer rotation;    
}


