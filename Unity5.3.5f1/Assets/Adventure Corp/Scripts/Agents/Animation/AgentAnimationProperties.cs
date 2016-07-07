using UnityEngine;
using System.Collections;


public class AgentAnimationProperties : MonoBehaviour
{
    public Animation animatedGameObject;
    public Mesh gizmoMesh;
    public Color gizmoColor = Color.red;

    [Range(0,1)]
    public float walkToRunSpeedRatio = 0.25f;
    public AnimationClipProperties idle;
    public AnimationClipProperties walk;
    public AnimationClipProperties run;

    public AnimationClipProperties death;
    public AnimationClipProperties hitFromBehind;
    public AnimationClipProperties hitFromFront;
    public AnimationClipProperties hitFromLeft;
    public AnimationClipProperties hitFromRight;

    public AnimationClipProperties GetRandom(AnimationClipProperties[] clipProperties)
    {
        return clipProperties[Random.Range((int)0, clipProperties.Length - 1)];
    }
}
