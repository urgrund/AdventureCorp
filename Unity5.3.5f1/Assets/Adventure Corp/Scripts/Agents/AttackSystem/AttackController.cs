﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Main controller that holds the attack descriptors
/// </summary>
public class AttackController : MonoBehaviour
{

    public bool showDebugGUI = false;
	public LayerMask damageColliderLayer;// = LayerMask.NameToLayer("Everything");  

	// Agent owning this attack controller
	Agent _agent;

	// Target to turn towards whilst attacking
	Transform turnToTarget = null;
    
    [HideInInspector]
    public Damager[] damagers;
		
	//public AttackDescriptor[] attacks;
    AttackDescriptor _currentAttack;
	public AttackDescriptor currentAttack { get { return _currentAttack; } }

	float _currentAttackStartTime = 0;
	public float currentAttackStartTime { get { return _currentAttackStartTime; } }

    private bool _isAttacking = false;
    public bool isAttacking { get { return _isAttacking; } }

    private bool _isControllingAgentVelocity = false;
    public bool isControllingAgentVelocity { get { return _isControllingAgentVelocity; } }

	/// <summary>
	/// Length, in seconds, of the clip played in this attack
	/// </summary>
	public float currentAttackClipLength
	{
		get { return _currentAttack.clipProperties.clip.length * (1f / _currentAttack.clipProperties.playSpeed); }
	}

	/// <summary>
	/// When, in seconds, does this attack activate the damage volumes
	/// </summary>
	public float currentAttackValidDamageRangeStartTime
	{
		get { return currentAttackClipLength * _currentAttack.validDamageRange.x; }
	}


	// ------------------------------------------------------------------------------

	void Awake()
	{
		Debug.Assert(GetComponent<Agent>() != null, "No agent or animated object on " + this.name);
		_agent = GetComponent<Agent>();
		_agent.onStaggered += OnAgentStaggered; 
	}

	void Start()
    {
        damagers = AttackVolumeCollection.CreateDamageCollidersForAgent(this, _agent, _agent.animationController.animatedGameObject.transform, _agent.properties.GetComponent<AttackVolumeCollection>());
		//SetLayerToDamageVolumes(damageColliderLayer);
		Debug.LogWarning("TODO - Fix layer masking so enemies don't hit themselves");
    }

	// ------------------------------------------------------------------------------




	/// <summary>
	/// Returns if the attack animation is allowed to be broken
	/// </summary>
	public bool isPastYieldControlTime
    {
        get
        {
            if (isAttacking)
                return _agent.animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime > _currentAttack.yieldControlRatio;
            else
                return true;
        }
    }


	/// <summary>
	/// Pass an AttackDescriptor to begin attacking
	/// </summary>	
    public void AttackWithDescriptor(AttackDescriptor d, Transform target = null)
    {
		if (_agent.isStaggered)
			return;

        if (isPastYieldControlTime)
        {
            StopAllCoroutines();
			turnToTarget = target;
            StartCoroutine(RunJobRoutine(d));
            if (d.eventor != null)
                EventorSchedule.RunAtTransformAsChild(d.eventor, this.transform);
        }
    }	

	public void SetLayerToDamageVolumes(LayerMask layer)
	{
		for (int i = 0; i < damagers.Length; i++)
			damagers[i].gameObject.layer = layer;
	}

    // Apply the appropriate Damage object to the volumes    
    public void SetDamageToDamageVolumes(Damage damage)
    {
        for (int i = 0; i < damagers.Length; i++)
            damagers[i].damage = damage;
    }

    
    public void SetOwnerHealthToDamageVolumes(Health health)
    {        
        for (int i = 0; i < damagers.Length; i++)
            damagers[i].owner = health;        
    }


    // This assumes the attack indices match the NPC's damager indices
    // If there's a mistmatch it's most likely that the attack descriptor
    // was never opened in the editor and initialized
    public void ActivateDamageVolumes(bool isEnabled, bool[] volumeIndices)
    {
        for (int i = 0; i < damagers.Length; i++)
        {
            if (isEnabled)
                damagers[i].enabled = volumeIndices[i];
            else
                damagers[i].enabled = false;
            damagers[i].sphereCollider.enabled = damagers[i].enabled;
        }
    }


	/// <summary>
	/// Ask for the attack to stop playback and return control of the agent
	/// This can be forced, or on condition of the attacks yeild time
	/// </summary>
    public bool YieldControlFromAttack() { return YieldControlFromAttack(false); }
    public bool YieldControlFromAttack(bool force)
    {
		if (_isAttacking)
		{
			if (force)
			{
				ReleaseAttack(_currentAttack);
				return true;
			}
			else
			{

				if (isPastYieldControlTime)
				{
					ReleaseAttack(_currentAttack);
					return true;
				}
			}
		}
		return false;
    }

    void OnAgentStaggered()
    {
        if(isAttacking)
            ReleaseAttack(_currentAttack);
    }

    void ReleaseAttack(AttackDescriptor attack)
    {
		Debug.Assert(attack != null, "AttackDescriptor was null");

        // Deactivate
        ActivateDamageVolumes(false, attack.volumeIndices);
		_currentAttack = null;
        _agent.isAllowedMovement = true;
        _agent.isApplyGravity = true;
        _isAttacking = false;
        _isControllingAgentVelocity = false;
		turnToTarget = null;

		StopAllCoroutines();

		// Set override cooldown below zero so that
		// the animation controller can take over
        _agent.animationController.overrideCountDown = -1;
    }


	public bool CheckAttackInRangeForTarget(AttackDescriptor attack, Transform target)
	{
		return !Helpers.InRadius(transform.position, target.position, attack.suggestedUseRange.x) && Helpers.InRadius(transform.position, target.position, attack.suggestedUseRange.y);
	}

	public bool CheckAttackInAngleForTarget(AttackDescriptor attack, Transform target)
	{
		//print(attack.suggestedUseAngle + "     " + (int)attack.suggestedUseAngle);
		//print("target is " + target.name);
		Vector3 dirTo = Helpers.DirectionTo(transform, target);
		return (Vector3.Angle(dirTo, transform.forward) < (int)attack.suggestedUseAngle * 0.5f);
	}

	public bool CheckAttackIsValidForTarget(AttackDescriptor attack, Transform target)
	{
		//print("\n ------- Checking " + attack.name);
		return CheckAttackInAngleForTarget(attack, target) && CheckAttackInRangeForTarget(attack, target);
	}


	// TODO - 
	// Optimisation, call this for general scenarios (in front, behind... etc) 
	// then could use a lookup based on direction/distance 
	public List<AttackDescriptor> GetSuggestedAttacksForTarget(BaseAttackCollection collection, Transform target)
	{
		List<AttackDescriptor> attacks = null;

		for (int i = 0; i < collection.AsArray().Length; i++)
		{
			if (collection.AsArray()[i] == null)
				continue;
			
			if (CheckAttackIsValidForTarget(collection.AsArray()[i], target))
			{
				if (collection.AsArray()[i] != null)
				{
					if (attacks == null)
						attacks = new List<AttackDescriptor>();
					attacks.Add(collection.AsArray()[i]);
				}
			}
		}
		return attacks;
	}


    IEnumerator RunJobRoutine(AttackDescriptor attack)
    {
        // Setup volumes
        SetDamageToDamageVolumes(attack.damage);
        ActivateDamageVolumes(false, attack.volumeIndices);
        _currentAttack = attack;
		_currentAttackStartTime = Time.time;

		if (attack.controllerLock == AttackDescriptor.Lock.Curves)
            curveLastPosition = GetPositionOnCurve(_currentAttack, 0f);

        float t;
        // Play animation and activate volumes during damage range
        if (_agent.animationController.animatedGameObject != null && _currentAttack != null)
        {            
            _agent.animationController.Play(_currentAttack.clipProperties);
            while (_agent.animationController.animatedGameObject.IsPlaying(_currentAttack.clipProperties.clip.name))
            {
                // Activate volumes during the attack
                _isAttacking = true;
                t = _agent.animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > _currentAttack.validDamageRange.x && t < _currentAttack.validDamageRange.y, _currentAttack.volumeIndices);

				// Rotate to target if available
				if (turnToTarget != null)
				{
					if (t < _currentAttack.turnToTargetRatio)					
						_agent.SetDesiredRotation(Helpers.DirectionTo(this.transform, turnToTarget));					
				}

                // Control over agent if needed
                if (attack.controllerLock != AttackDescriptor.Lock.None)
                {
                    _isControllingAgentVelocity = true;
					_agent.isApplyGravity = _currentAttack.controllerGravity;
                    switch (attack.controllerLock)
                    {
                        case AttackDescriptor.Lock.StopAllMovement: _agent.isAllowedMovement = false; break;
                        case AttackDescriptor.Lock.ScaleMovementHalf: _agent.SetVelocityScaleThisFrame(0.50f); break;
                        case AttackDescriptor.Lock.ScaleMovementQuarter: _agent.SetVelocityScaleThisFrame(0.25f); break;
                        case AttackDescriptor.Lock.Curves: MoveAgentWithAttackCurves(_currentAttack, t); break;
                    }
                }            
                yield return null;
            }         
        }
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        ReleaseAttack(_currentAttack);
    }


    // Declare here so don't need to declare each frame
    private Vector3 curvePosition;
    private Vector3 curveLastPosition;
    private Vector3 curveDirection;
    void MoveAgentWithAttackCurves(AttackDescriptor attack, float normalizedTime)
    {
        curvePosition = GetPositionOnCurve(attack, normalizedTime);
        curveDirection = _agent.transform.TransformDirection(curvePosition - curveLastPosition);

		Debug.DrawRay(transform.position + Vector3.up, curveDirection, Color.red, 1f);

        if(curveDirection.magnitude > 0)
            _agent.OverrideMove(curveDirection);

        curveLastPosition = curvePosition;        
    }

    Vector3 GetPositionOnCurve(AttackDescriptor attack, float normalizedTime)
    {
        curvePosition = Vector3.zero;
        curvePosition.x = attack.curveX.Evaluate(normalizedTime);
        curvePosition.y = attack.curveY.Evaluate(normalizedTime);
        curvePosition.z = attack.curveZ.Evaluate(normalizedTime);
        return ( curvePosition );
    }	


    
    void OnDrawGizmos()
    {
        if (damagers == null)
            return;

        foreach (Damager d in damagers)
        {
            if (d.GetComponent<SphereCollider>())
            {                
                SphereCollider c = d.GetComponent<SphereCollider>();
                Gizmos.color = d.enabled ? Color.red : Color.grey;
                Gizmos.DrawWireSphere(d.transform.TransformPoint(c.center), c.radius);
            }
        }
    }
}
