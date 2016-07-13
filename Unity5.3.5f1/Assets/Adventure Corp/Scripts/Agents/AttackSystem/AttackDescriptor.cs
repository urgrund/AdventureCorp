using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "atk_AttackDescriptor", menuName = "Adventure Corp/Attack Descriptor")]
public class AttackDescriptor : ScriptableObject
{
    /// <summary>
    /// Parameterized angles of attack for consistency
    /// </summary>
    public enum Angle
    {
        Narrow = 15,
        Wide = 90,
        Breadth = 180,
        Encompasse = 360
    }

    /// <summary>
    /// Lock properties for how an attack effects the underlaying Agent
    /// </summary>
    public enum Lock
    {
        None,                   // Agent controller is not effected
        ScaleMovementHalf,      // Agent controllers' speed is scaled
        ScaleMovementQuarter,
        StopAcceleration,       // Agent will stop accelerating and therefore slow down
        Curves,                 // Agent will be moved by the attack curves
        StopAllMovement         // Agent controller is completely locked during attack
    }


    public bool canBeBroken = false;    
    public Lock controllerLock = Lock.None;    
    
    [Range(0,360)]public float suggestedUseAngle = 90f;
    [Range(0, 1)]public float yieldControlRatio = 1f;

    public Vector2 suggestedUseRange = new Vector2(0.5f, 3f);

    [HideInInspector]
    public Vector2 validDamageRange = new Vector2(0.25f, 0.75f);
    public Damage damage;

    [HideInInspector]
    public bool[] volumeIndices = new bool[0];

    [HideInInspector]
    public AnimationCurve curveX = AnimationCurve.Linear(0, 0, 1, 0);
    [HideInInspector]
    public AnimationCurve curveY = AnimationCurve.Linear(0, 0, 1, 0);
    [HideInInspector]
    public AnimationCurve curveZ = AnimationCurve.Linear(0, 0, 1, 0);

    public AnimationClipProperties clipProperties;
    public EventorSchedule eventor; 
}