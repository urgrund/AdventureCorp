using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "atk_AttackDescriptor", menuName = "Adventure Corp/Attack Descriptor")]
public class AttackDescriptor : ScriptableObject
{
    public enum Angle
    {
        Narrow = 15,
        Wide = 90,
        Breadth = 180,
        Encompasse = 360
    }

    public bool canBeBroken = false;
    public bool useCurves = false;
    public Vector2 suggestedUseRange = new Vector2(0.5f, 3f);
    [Range(0,360)]
    public float suggestedUseAngle = 90f;
    public Vector2 validDamageRange = new Vector2(0.25f, 0.75f);
    public Damage damage;
    public bool[] volumeIndices = new bool[0];
    public AnimationClipProperties clipProperties;    
}