using UnityEngine;

//[System.Serializable]
[CreateAssetMenu(fileName = "AP_Anim_Clip_Properties", menuName = "Adventure Corp/Animation Clip Properties")]
public class AnimationClipProperties : ScriptableObject
{ 
    public AnimationClip clip;
    public float playSpeed = 1f;
    public float blendTime = 0.2f;
    public AnimationBlendMode blendMode = AnimationBlendMode.Blend;
    public PlayMode playMode = PlayMode.StopAll;
    public Animation animatedGameObject;
}
