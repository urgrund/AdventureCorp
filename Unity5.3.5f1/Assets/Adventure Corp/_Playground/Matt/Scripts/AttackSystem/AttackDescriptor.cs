using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ad_AttackDescriptor", menuName = "Adventure Corp/Attack Descriptor")]
public class AttackDescriptor : ScriptableObject
{
    public bool canBeBroken = false;
    public Vector2 minMaxRange = new Vector2(0, 2);
    public Vector2 coolDownRange = new Vector2(0, 1);
    public Damage damage;


    public AnimationClipProperties[] animations;
}