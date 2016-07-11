using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public sealed class Agent : MonoBehaviour
{
    private CharacterController _controller;
    public CharacterController controller { get { return _controller; } }

    /// <summary>
    /// Properties to define the movement properties of this agent
    /// </summary>
    public AgentProperties properties;    

    private Health _health;
    public Health health { get { return _health; } }

    private float gravitySpeed = 0f;


    Vector3 _currentVelocity;       // Velocity the Controller will move
    Vector3 _desiredVelocity;       // Velocity the Controller will try to reach
    Quaternion _desiredRotation;    
    private bool _isBrainSetVelocityThisFrame = false;

    public bool isAllowedMovement = true;
    public bool isAllowedRotation = true;

    /// <summary>
    /// Ratio of the agents max speed to current speed.  This takes into account only XZ
    /// </summary>
    public float speedRatio
    {
        get
        {
            Vector3 v = _controller.velocity;
            v.y = 0;
            return v.magnitude / properties.speed.max;
        }
    }

    /// <summary>
    /// Set the default parameters for an Agent
    /// This will bias toward our particular needs in ACorp
    /// </summary>
    void Reset()
    {
        _controller = GetComponent<CharacterController>();
        _controller.center = Vector3.up;
        _controller.skinWidth = 0.025f;
        _controller.stepOffset = 0.5f;
    }

    void Awake()
    {
        // Initial headings
        _desiredVelocity = Vector3.zero;
        _desiredRotation = transform.rotation;

        // Setup component references
        _controller = GetComponent<CharacterController>();
        _health = GetComponent<Health>();
        _health.onHealthLost += OnHealthLost;
        _health.onHealthZero += OnHealthLost;
    }

    void Update()
    {
        UpdateMovement();
        UpdateRotation();
    }
        

    void UpdateMovement()
    {
        if (isAllowedMovement)
        {
            // Apply gravity
            if (!controller.isGrounded)
                gravitySpeed -= properties.gravity;
            else
                gravitySpeed = -controller.stepOffset / Time.deltaTime * 5;
            
            _currentVelocity.y = gravitySpeed * Time.deltaTime;
            _controller.Move(_currentVelocity * Time.deltaTime);            
        }
    }

    void UpdateRotation()
    {
        if (isAllowedRotation)
        {
            transform.rotation = _desiredRotation;
        }
    }

    
    void LateUpdate()
    {
        // Apply damping to velocity if no Brain input
        if (!_isBrainSetVelocityThisFrame && controller.isGrounded)
        {
            Vector3 cV = _currentVelocity;
            cV = Vector3.MoveTowards(_currentVelocity, Vector3.zero, properties.speed.damping * Time.deltaTime);
            cV.y = _currentVelocity.y;
            _currentVelocity = cV;
        }
        _isBrainSetVelocityThisFrame = false;
    }

    


    /// <summary>
    /// Rotate agent to look along the velocity vector using rotation speed
    /// </summary>    
    public Quaternion RotateToVelocityDirection(float rotationSpeed)
    {
        Vector3 v = _desiredVelocity;
        v.y = 0;
        return Quaternion.Lerp(transform.rotation, MathLab.CreateRotationToLookAt(v.normalized + transform.position, transform.position), rotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Rotate agent to look at a target position using rotation speed
    /// </summary>    
    public Quaternion RotateToLookAt(Vector3 target, float rotationSpeed)
    {
        return Quaternion.Lerp(transform.rotation, MathLab.CreateRotationToLookAt((target - transform.position).normalized + transform.position, transform.position), rotationSpeed * Time.deltaTime);
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

        // Blend between accelearation & damping based on dot of current/desired
        // this is so that if the desired direction is BEHIND you, you use damping values
        // to first STOP then Accelerate 
        float dotWithVel = (2 + Vector3.Dot(_controller.velocity.normalized, transform.forward)) * 0.5f;
        Vector3 targetVelocity = Vector3.Lerp(Vector3.zero, velocity, dotWithVel);
        //v = Vector3.MoveTowards(v, targetVelocity, properties.speed.acceleration * Time.deltaTime);

        v = Vector3.ClampMagnitude(v, properties.speed.max);
        _desiredVelocity = v;

//        print("Des : " + _desiredVelocity + "    Cur: " + _currentVelocity);
        _currentVelocity = Vector3.MoveTowards(_currentVelocity, _desiredVelocity, properties.speed.acceleration);
        _isBrainSetVelocityThisFrame = true;
    }



    /// <summary>
    /// Set player velocity and rotate the character towards velocity direction. If no rotation is needed set the bool isRotate to false
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, bool isRotate)
    {        
        CalculateDesiredVelocity(velocity);

        if (_desiredVelocity != Vector3.zero && isRotate)
            _desiredRotation = RotateToVelocityDirection(properties.rotation.max);
    }



    /// <summary>
    /// Set player velocity and rotate the character towards a vector3 target
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, Vector3 lookAtTarget)
    {
        CalculateDesiredVelocity(velocity);
        _desiredRotation = RotateToLookAt(lookAtTarget, properties.rotation.max);
    }


    public void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        if (_health.isDead)
        {
            print("Died");
            controller.detectCollisions = false;
            controller.enabled = false;
            this.enabled = false;
        }
    }


    // TODO - This should go in the animation controller as its 
    // visual feedback, not Agent related
        
    //IEnumerator DieRotateRoutine(Vector3 directionLastHit)
    //{
    //    directionLastHit.y = 0;
    //    directionLastHit.Normalize();
    //    Quaternion rotTarget = Quaternion.LookRotation(directionLastHit, Vector3.up);        

    //    float t = 0;
    //    float speed = 8f;
    //    while (t < 1f)
    //    {
    //        t += Time.deltaTime * speed;
    //        _desiredRotation = Quaternion.Lerp(transform.rotation, rotTarget, Time.deltaTime * speed );
    //        yield return null;
    //    }
    //}        
    
}
