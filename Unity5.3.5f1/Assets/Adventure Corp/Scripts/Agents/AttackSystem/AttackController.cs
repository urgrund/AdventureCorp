using UnityEngine;
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
	Transform _turnToTarget = null;
    
    [HideInInspector]
    public Damager[] damagers;
		
    private bool _isAttacking = false;
    public bool isAttacking { get { return _isAttacking; } }

    private bool _isControllingAgentVelocity = false;
    public bool isControllingAgentVelocity { get { return _isControllingAgentVelocity; } }



	AttackDescriptor _currentAttack;
	public AttackDescriptor currentAttack { get { return _currentAttack; } }

	float _currentAttackStartTime = 0;
	public float currentAttackStartTime { get { return _currentAttackStartTime; } }

	/// <summary>
	/// Length, in seconds, of the clip played in this attack
	/// </summary>
	public float currentAttackClipLength
	{
		get { return _currentAttack.clipProperties.clip.length * (1f / _currentAttack.clipProperties.playSpeed); }
	}

	/// <summary>
	/// Normalized time into this attack
	/// </summary>
	public float currentAttackTime { get { return t; } }

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


		
	public bool isPastYieldControlTime
    {
        get
		{
			if (_isAttacking && _currentAttack != null)
			{
				float localYieldTime = (_currentAttack.clipProperties.scaledLength * _currentAttack.yieldControlRatio);
				float startOffset = _currentAttack.clipProperties.scaledLength * _currentAttack.attackStartOffsetRatio;
				float trueTimeYield = _currentAttackStartTime + (localYieldTime - startOffset);

				return Time.time > (_currentAttackStartTime + trueTimeYield);
			}
			else
				return true;


			//if (_isAttacking)
			//    return _agent.animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime > _currentAttack.yieldControlRatio;
			//else
			//    return true;
		}
    }

	public bool isPastTurnToTime
	{
		get
		{
			if (_isAttacking)
				return t > _currentAttack.turnToTargetRatio;
			else
				return true;
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
    public bool YieldControlFromAttack(bool force=false)
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
		if (attack != null)
		{	
			ActivateDamageVolumes(false, attack.volumeIndices);
			_agent.animationController.Stop(attack.clipProperties);
		}

		_currentAttack = null;
        _agent.isAllowedMovement = true;
        _agent.isApplyGravity = true;
        _isAttacking = false;
        _isControllingAgentVelocity = false;
		_turnToTarget = null;		
		
		StopAllCoroutines();

		// Set override cooldown below zero so that
		// the animation controller can take over
        _agent.animationController.overrideCountDown = -1;
    }


	public bool IsAttackInRangeForTarget(AttackDescriptor attack, Transform target)
	{
		return !Helpers.InRadius(transform.position, target.position, attack.suggestedUseRange.x) && Helpers.InRadius(transform.position, target.position, attack.suggestedUseRange.y);
	}

	public bool IsAttackInAngleForTarget(AttackDescriptor attack, Transform target)
	{
		Vector3 dirTo = Helpers.DirectionTo(transform, target);
		return (Vector3.Angle(dirTo, transform.forward) < (int)attack.suggestedUseAngle * 0.5f);
	}

	public bool IsAttackIsValidForTarget(AttackDescriptor attack, Transform target)
	{
		return IsAttackInAngleForTarget(attack, target) && IsAttackInRangeForTarget(attack, target);
	}



	public List<AttackDescriptor> GetSuggestedAttacksForTarget(BaseAttackCollection collection, Transform target) { return GetSuggestedAttacksForTarget(collection.AsArray(), target); }
	public List<AttackDescriptor> GetSuggestedAttacksForTarget(AttackDescriptor[] attackCollectionArray, Transform target, float maxDistanceToTarget=Mathf.Infinity)
	{
		List<AttackDescriptor> attacks = null;

		for (int i = 0; i < attackCollectionArray.Length; i++)
		{
			if (attackCollectionArray[i] == null)
				continue;

			// If the farthest distance for this attack is greater than
			// the max distance to target then ignore this attack
			if (attackCollectionArray[i].suggestedUseRange.y > maxDistanceToTarget)
				continue;
			
			if (IsAttackIsValidForTarget(attackCollectionArray[i], target))
			{
				if (attackCollectionArray[i] != null)
				{
					if (attacks == null)
						attacks = new List<AttackDescriptor>();
					attacks.Add(attackCollectionArray[i]);
				}
			}
		}
		return attacks;
	}



	/// <summary>
	/// Pass an AttackDescriptor to begin attacking
	/// </summary>	
	public void AttackWithDescriptor(AttackDescriptor d, Transform target = null)
	{
		if (_agent.isStaggered)
			return;

		if (d == null)
			return;
		
		if (isPastYieldControlTime)
		{
			ReleaseAttack(_currentAttack);
			_turnToTarget = target;
			_currentAttack = d;
			_currentAttackStartTime = Time.time;
			_isAttacking = true;		
			_agent.animationController.Play(_currentAttack.clipProperties, _currentAttack.attackStartOffsetRatio);
			StartCoroutine(RunJobRoutine(d));
			if (d.eventor != null)
				EventorSchedule.RunAtTransformAsChild(d.eventor, this.transform);
		}
	}

	private float t;
	public float _currentAttackTotalPlayTime;
	IEnumerator RunJobRoutine(AttackDescriptor attack)
    {
        // Setup volumes
        SetDamageToDamageVolumes(attack.damage);
        ActivateDamageVolumes(false, attack.volumeIndices);        

		if (attack.controllerLock == AttackDescriptor.Lock.Curves)
            curveLastPosition = GetPositionOnCurve(_currentAttack, 0f);

		//t = _currentAttack.attackStartOffsetRatio;
		//_currentAttackTotalPlayTime = attack.clipProperties.scaledLength;
		
        // Play animation and activate volumes during damage range
        if (_agent.animationController.animatedGameObject != null && _currentAttack != null)
        {
			float totalPlayTime = 0f;
			totalPlayTime += attack.clipProperties.scaledLength * attack.yieldControlRatio;
			totalPlayTime -= attack.clipProperties.scaledLength * attack.attackStartOffsetRatio;			

			float runTime = 0f;
			//while (_agent.animationController.animatedGameObject.IsPlaying(_currentAttack.clipProperties.clip.name))
			//while (Time.time < _currentAttackStartTime + totalPlayTime)
			while(runTime < totalPlayTime)
            {
				runTime += Time.deltaTime;
				_isAttacking = true;
				
				// Activate volumes during the attack				
                t = _agent.animationController.animatedGameObject[_currentAttack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > _currentAttack.validDamageRange.x && t < _currentAttack.validDamageRange.y, _currentAttack.volumeIndices);

				// Rotate to target if available
				if (_turnToTarget != null)
				{
					if (t < _currentAttack.turnToTargetRatio)					
						_agent.SetDesiredRotation(Helpers.DirectionTo(this.transform, _turnToTarget));					
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
