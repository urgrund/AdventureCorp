using UnityEngine;
using System.Collections;

[System.Serializable]
public class AgentAnimationPropertiesForLocomotion
{
    public AnimationClipProperties idle;
    public AnimationClipProperties walk;
    public AnimationClipProperties run;
    public AnimationClipProperties backPedal;
    public AnimationClipProperties strafeLeft;
    public AnimationClipProperties strafeRight;
    public AnimationClipProperties jump;
}

[System.Serializable]
public class AgentAnimationPropertiesForDamage
{
    public AnimationClipProperties spawn;
    public AnimationClipProperties death;
    public AnimationClipProperties hitFromBehind;
    public AnimationClipProperties hitFromFront;
    public AnimationClipProperties hitFromLeft;
    public AnimationClipProperties hitFromRight;
}



public class AgentAnimationProperties : MonoBehaviour
{
    public Animation animatedGameObject;
    public Mesh gizmoMesh;
    public Color gizmoColor = Color.red;

    public AgentAnimationPropertiesForLocomotion locomotion;
    public AgentAnimationPropertiesForLocomotion damage;

    [Range(0,1)]
    public float walkToRunSpeedRatio = 0.25f;
    public AnimationClipProperties spawn;
    public AnimationClipProperties idle;
    public AnimationClipProperties walk;
    public AnimationClipProperties run;

    public AnimationClipProperties death;
    public AnimationClipProperties hitFromBehind;
    public AnimationClipProperties hitFromFront;
    public AnimationClipProperties hitFromLeft;
    public AnimationClipProperties hitFromRight;
}
