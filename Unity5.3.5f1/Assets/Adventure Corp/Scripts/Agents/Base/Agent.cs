using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public sealed class Agent : MonoBehaviour
{
    private CharacterController _controller;
	public bool isGrounded { get { return _controller.isGrounded; } }
    public CharacterController controller { get { return _controller; } }

    private AgentAnimationController _animationController;
    public AgentAnimationController animationController { get { return _animationController; } }

    /// <summary>
    /// Properties to define the movement properties of this agent
    /// </summary>
    public AgentProperties properties;    

    private Health _health;
    public Health health { get { return _health; } }

    private float gravitySpeed = 0f;

    private float _currentVelocityScale = 1f;
    public void SetVelocityScaleThisFrame(float value) { _currentVelocityScale = value; }

    
    Vector3 _currentVelocity;       // Velocity the Controller will move
    Vector3 _desiredVelocity;       // Velocity the Controller will try to reach    
    Quaternion _desiredRotation;    // TODO - need a current rotate speed so that DAMPING and ACCEL can be used properly

    private bool _isBrainSetVelocityThisFrame = false;

    [HideInInspector]
    public bool isAllowedMovement = true;
    [HideInInspector]
    public bool isAllowedRotation = true;
    [HideInInspector]
    public bool isApplyGravity = true;

    public enum MoveDirection
    {
        Stationary,
        Foward,
        Back,
        Left,
        Right
    }
    private MoveDirection _moveDirection = MoveDirection.Stationary;
    public MoveDirection moveDirection { get { return _moveDirection; } }


    /// <summary>
    /// Ratio of the agents max speed to current speed.  This takes into account only XZ and 
    /// scales the result by the relative movement.  This is to give a fairer ratio where backpedal
    /// may have a 0.25 speed scale, which means when speed is at 0.25, it's actaully "max" 
    /// </summary>
    public float speedRatio
    {
        get
        {
            Vector3 v = _controller.velocity;
            v.y = 0;
            return v.magnitude / (properties.speed.max * _desiredVelocityRealtiveMovementScale);
        }
    }
    


    /// <summary>
    /// Dot product between agent facing direction and velocity. Agent uses 
    /// this to determine movement speeds. 
    /// </summary>
    public float velocityRelativeToRotation { get { return Vector3.Dot(_controller.velocity.normalized, transform.forward); } }
    private float _desiredVelocityRealtiveMovementScale = 1f;    

    /// <summary>
    /// Set the default parameters for an Agent
    /// This will bias toward our particular needs in ACorp
    /// </summary>
    void Reset()
    {
        _animationController = GetComponent<AgentAnimationController>();
        _controller = GetComponent<CharacterController>();
        _controller.center = Vector3.up;
        _controller.skinWidth = AdventureCorpGlobals.Agent.skinWidth;
        _controller.stepOffset = AdventureCorpGlobals.Agent.stepOffset;
    }

    void Awake()
    {
        // Initial headings
        _currentVelocity = _desiredVelocity = Vector3.zero;
        _desiredRotation = transform.rotation;

        // Setup component references
        Reset();
        _health = GetComponent<Health>();
        _health.onHealthLost += OnHealthLost;
        _health.onHealthZero += OnHealthLost;
    }

    void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }



	// ---------------------------------------------------------------------    
	// Overriding movement allows for ways to override the agents control 

	bool _isOverrideMoveThisFrame = false;
	bool _isOverrideTimedMoveThisFrame = false;
	public bool isOverrideMoveThisFrame { get { return _isOverrideMoveThisFrame || _isOverrideTimedMoveThisFrame; } }
	IEnumerator _overrideMoveRoutine;

	public void OverrideMove(Vector3 velocity)
    {        
        _controller.Move(velocity);
        _isOverrideMoveThisFrame = true;
    }

	public void OverrideMove(Vector3 velocity, float time)
	{
		if (_overrideMoveRoutine != null)
		{
			StopCoroutine(_overrideMoveRoutine);
			_isOverrideTimedMoveThisFrame = false;
		}
		_overrideMoveRoutine = OverrideMoveRoutine(velocity, time);
		StartCoroutine(_overrideMoveRoutine);
	}

	private IEnumerator OverrideMoveRoutine(Vector3 velocity, float time)
	{
		float t = 0;
		while (t < time)
		{
			t += Time.deltaTime;
			ApplyGravityToVelocity(ref velocity);
			_controller.Move(velocity);
			_isBrainSetVelocityThisFrame = true;
			_isOverrideMoveThisFrame = true;
			_isOverrideTimedMoveThisFrame = true;
			yield return null;
		}
		_isOverrideTimedMoveThisFrame = false;
	}
	// ---------------------------------------------------------------------    






	// ---------------------------------------------------------------------    
	// S T A G G E R 

	bool _isStaggered = false;
    public bool isStaggered { get { return _isStaggered; } }    
    float _staggerTimeCount = 0f;

    public delegate void OnStaggered();
    public OnStaggered onStaggered;

	/// <summary>
	/// Stagger the Agent, restricting movement 
	/// </summary>
	public void Stagger(float time = 3)
    {
        if (!_isStaggered)
        {
            if (onStaggered != null)
                onStaggered();

            _isStaggered = true;
            _staggerTimeCount = time;
            animationController.Play(animationController.animationProperties.reaction.stagger);
            StartCoroutine(StaggerRoutine());
			if (_overrideMoveRoutine != null)
				StopCoroutine(_overrideMoveRoutine);
        }
    }

    private IEnumerator StaggerRoutine()
    {
        yield return new WaitForSeconds(_staggerTimeCount);
        _isStaggered = false;
    }
    // ---------------------------------------------------------------------




    void UpdateMovement()
    {
        if (isAllowedMovement && !isOverrideMoveThisFrame)
        {
			// Apply gravity
			ApplyGravityToVelocity(ref _currentVelocity);

            UpdateMoveDirectionState();            
            _controller.Move(_currentVelocity * Time.deltaTime * _currentVelocityScale);
            _currentVelocityScale = 1f;
        }
    }

	void ApplyGravityToVelocity(ref Vector3 velocity)
	{
		// Apply gravity
		if (isApplyGravity)
		{
			if (!_controller.isGrounded)
				gravitySpeed -= AdventureCorpGlobals.Agent.gravity;
			else
				gravitySpeed = -_controller.stepOffset / Time.deltaTime * 5;

			velocity.y = gravitySpeed * Time.deltaTime;
		}
	}


    /// <summary>
    ///  Adjust the velocity scale based on rotation in order to adjust
    ///  speed for things like strafing and backpedalling
    /// </summary>
    void UpdateMoveDirectionState()
    {
        _desiredVelocityRealtiveMovementScale = 1f;
        if (_controller.velocity.magnitude > 0.01f) 
        {
            if (velocityRelativeToRotation < 0.707f)
            {
                if (velocityRelativeToRotation < -0.707f)
                {
                    _moveDirection = MoveDirection.Back;
                    _desiredVelocityRealtiveMovementScale = properties.backPedalSpeedFactor;
                }
                else
                {
                    if (transform.InverseTransformDirection(_controller.velocity).x < 0)
                        _moveDirection = MoveDirection.Left;
                    else
                        _moveDirection = MoveDirection.Right;
                    _desiredVelocityRealtiveMovementScale = properties.strafeSpeedFactor;
                }
            }
            else
                _moveDirection = MoveDirection.Foward;
        }
        else
            _moveDirection = MoveDirection.Stationary;
    }



    void UpdateRotation()
    {
        if (isAllowedRotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, properties.rotation.max * Time.deltaTime);
        }
    }

    
    void LateUpdate()
    {
        // Apply damping to velocity if no Brain input
        if (!_isBrainSetVelocityThisFrame && !isOverrideMoveThisFrame && _controller.isGrounded)
        {
            Vector3 cV = _currentVelocity;
            cV = Vector3.MoveTowards(_currentVelocity, Vector3.zero, properties.speed.damping * Time.deltaTime);
            cV.y = _currentVelocity.y;
            _currentVelocity = cV;
        }
        _isBrainSetVelocityThisFrame = false;
        _isOverrideMoveThisFrame = false;
    }

    
    

    /// <summary>
    /// Will start the Agent moving along the velocity passed in using acceleration properties
    /// </summary>
    void CalculateDesiredVelocity(Vector3 velocity)
    {
        // TODO - might be interesting to consider different surfaces
        // such as ice.  This could simply be scalars to acceleartion/damping etc?
        Vector3 v = velocity;
        v.y = 0;
        v = Vector3.ClampMagnitude(v, properties.speed.max);
        _desiredVelocity = v;

        // Scale the desired velocity based on move direction
        UpdateMoveDirectionState();
        _desiredVelocity *= _desiredVelocityRealtiveMovementScale;

        float moveToSpeed = (_currentVelocity.magnitude > _desiredVelocity.magnitude) ? properties.speed.damping : properties.speed.acceleration;
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, _desiredVelocity, moveToSpeed);
        _isBrainSetVelocityThisFrame = true;
    }



    public void SetDesiredRotation(Vector3 direction, bool immediate = false) { SetDesiredRotation(Quaternion.LookRotation(direction, Vector3.up), immediate); }
    public void SetDesiredRotation(Transform lookAtTarget, bool immediate = false) { SetDesiredRotation(Helpers.DirectionTo(transform, lookAtTarget), immediate); }
    public void SetDesiredRotation(Quaternion rotation, bool immediate = false)
    {
        _desiredRotation = rotation;
        if (immediate)
            transform.rotation = _desiredRotation;
    }


    /// <summary>
    /// Set player velocity and rotate the character towards velocity direction. If no rotation is needed set the bool isRotate to false
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, bool isRotate)
    {        
        CalculateDesiredVelocity(velocity);

        if (_desiredVelocity != Vector3.zero && isRotate)
            SetDesiredRotation(velocity.normalized);
    }

    /// <summary>
    /// Set player velocity and rotate the character towards a vector3 target
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, Vector3 lookAtTarget)
    {
        CalculateDesiredVelocity(velocity);
        SetDesiredRotation(lookAtTarget);        
    }


    public void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        if (_health.isDead)
        {
            print("Died");
            _controller.detectCollisions = false;
            _controller.enabled = false;
            this.enabled = false;
        }
        else
        {            
            if (info.value > properties.pushBackDamageThreshold) 
                OverrideMove(-Helpers.DirectionTo(transform, info.responsibleGameObject.transform) * 0.75f);
        }
    }
}
