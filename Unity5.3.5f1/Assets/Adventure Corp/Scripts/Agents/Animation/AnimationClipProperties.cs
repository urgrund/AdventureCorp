using UnityEngine;

[CreateAssetMenu(fileName = "AP_Anim_Clip_Properties", menuName = "Adventure Corp/Animation Clip Properties")]
public class AnimationClipProperties : ScriptableObject
{
    public enum Layer
    {
        Base = 1,
        Two = 2,
        Three = 3
    }

    public AnimationClip clip;
    public float playSpeed = 1f;
    public float blendTime = 0.2f;
    [Range(0,1)]
    public float weight = 1f;        
    public bool isOverriding = false;
    public bool isMixingTransform = false;
    public AnimationBlendMode blendMode = AnimationBlendMode.Blend;
    public Layer layer = Layer.Base;


    // These references assist with editor side configuration
    public Agent agent;
    public AgentAnimationProperties agentAnimationProperties; 
}
