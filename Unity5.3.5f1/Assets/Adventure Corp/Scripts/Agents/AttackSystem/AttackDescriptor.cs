using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ad_AttackDescriptor", menuName = "Adventure Corp/Attack Descriptor")]
public class AttackDescriptor : ScriptableObject
{
    public bool canBeBroken = false;
    public Vector2 minMaxRange = new Vector2(0.5f, 3f);
    public Vector2 coolDownRange = new Vector2(0, 1);
    public Vector2 validDamageRange = new Vector2(0.25f, 0.75f);
    public Damage damage;

    // Volumes that will be used for this attack
    public List<AttackVolumeDescriptor> volumes = new List<AttackVolumeDescriptor>();

    public AnimationClipProperties clipProperties;    
}