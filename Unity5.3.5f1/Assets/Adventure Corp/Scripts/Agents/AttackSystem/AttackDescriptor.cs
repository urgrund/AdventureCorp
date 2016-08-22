using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "atk_AttackDescriptor", menuName = "Adventure Corp/Attack Descriptor")]
public class AttackDescriptor : ScriptableObject
{

	// --------------------------------------------------------------------------
	// Attack Descriptor Members

	public bool canAttackBeParried = true;
	public Lock controllerLock = Lock.None;
	public bool controllerGravity = true;
		
	public Angle suggestedUseAngle = Angle.Breadth;

	// Start playing this animation this far in already
	[Range(0, 1)]
	public float attackStartOffsetRatio = 0f;

	// The attack can yield control back to another animation after this
	[Range(0, 1)]
	public float yieldControlRatio = 1f;

	// The agent will turn towards a target Transform 
	[Range(0, 1)]
	public float turnToTargetRatio = 0f;

	// Scalar this attack has on pushback 
	public float pushBackScale = 1f;

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

	// --------------------------------------------------------------------------



	/// <summary>
	/// Parameterized angles of attack for consistency
	/// </summary>
	public enum Angle
    {
        Narrow = 15,
        Wide = 90,
        Breadth = 180,
        Encompass = 360
    }

	// really annoying to do this because of Serialized Properties in the editor :(
	// the angles enum already maps to integers, but can't find that in a serialized
	// property, so need to create an array below that maps index to angle :(
	private static int[] angles = new int[4] { 15, 90, 180, 360 };
	public static int AngleFromIndex(int i) { return angles[i]; }
	
    /// <summary>
    /// Lock properties for how an attack effects the underlaying Agent
    /// </summary>
    public enum Lock
    {
        None,                   // Agent controller is not effected
        ScaleMovementHalf,      // Agent controllers' speed is scaled
        ScaleMovementQuarter,        
        Curves,                 // Agent will be moved by the attack curves
        StopAllMovement         // Agent controller is completely locked during attack
    }
}