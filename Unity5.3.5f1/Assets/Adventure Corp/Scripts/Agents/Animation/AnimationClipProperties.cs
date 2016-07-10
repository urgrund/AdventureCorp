using UnityEngine;

[CreateAssetMenu(fileName = "AP_Anim_Clip_Properties", menuName = "Adventure Corp/Animation Clip Properties")]
public class AnimationClipProperties : ScriptableObject
{ 
    public AnimationClip clip;
    public float playSpeed = 1f;
    public float blendTime = 0.2f;
    public float weight = 1f;    
    public AnimationBlendMode blendMode = AnimationBlendMode.Blend;
    //public PlayMode playMode = PlayMode.StopAll;
    public bool isOverriding = false;    

    // These references assist with editor side configuration
    public Agent agent;
    public AgentAnimationProperties agentAnimationProperties; 
}
