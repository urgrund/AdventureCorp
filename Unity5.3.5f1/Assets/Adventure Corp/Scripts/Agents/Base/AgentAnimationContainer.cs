using UnityEngine;
using System.Collections;

[System.Serializable]    
public class AgentAnimationClipProperties
{
    public AnimationClip clip;
    public float playSpeed = 1f;
    public float blendSpeed = 0.2f;
}


[System.Serializable]
public class AgentAnimationProperties
{
    public Animation animatedGameObject;
    public Mesh gizmoMesh;
    public Color gizmoColor = Color.red;
    [Range(0,1)]
    public float walkToRunSpeedRatio = 0.25f;
    public AgentAnimationClipProperties idle;
    public AgentAnimationClipProperties walk;
    public AgentAnimationClipProperties run;
}
