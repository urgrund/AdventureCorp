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
public class AgentAnimationPropertiesForReaction
{
    public AnimationClipProperties spawn;
    public AnimationClipProperties death;
    public AnimationClipProperties hitFromBehind;
    public AnimationClipProperties hitFromFront;
    public AnimationClipProperties hitFromLeft;
    public AnimationClipProperties hitFromRight;
}


/// <summary>
/// Container class for all animation clip references
/// </summary>
public class AgentAnimationProperties : MonoBehaviour
{
    public Animation animatedGameObject;
    public Mesh gizmoMesh;
    public Color gizmoColor = Color.red;

    public AgentAnimationPropertiesForLocomotion locomotion;
    public AgentAnimationPropertiesForReaction reaction;
}
