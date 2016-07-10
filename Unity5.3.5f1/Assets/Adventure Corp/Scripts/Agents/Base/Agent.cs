using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Health))]
public sealed class Agent : MonoBehaviour
{
    private CharacterController _controller;
    public CharacterController controller { get { return _controller; } }

    public AgentProperties properties;
    //public AgentAnimationProperties animationProperties;

    //private Animation _animatedGameObject;
    //public Animation animatedGameObject { get { return _animatedGameObject; } }

    private Health _health;
    public Health health { get { return _health; } }

    private float gravitySpeed = 0f;

    Vector3 _desiredVelocity;    
    Quaternion _desiredRotation;
    //private bool _isWithAnimatedObject = false;
    private bool _isBrainSetVelocityThisFrame = false;

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
        //if (animationProperties != null)
        //{
        //    if (animationProperties.animatedGameObject != null)
        //    {
        //        _animatedGameObject = Helpers.InstantiateAndParent(animationProperties.animatedGameObject.gameObject.transform, transform, true).GetComponent<Animation>();
        //        _isWithAnimatedObject = true;
        //    }
        //}
        //else
        //    Debug.LogError(name.ToString() + " has no animation properties.");


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
        UpdateController();
        //UpdateAnimations();
    }


    void UpdateController()
    {
        // Apply gravity
        if (!controller.isGrounded)
            gravitySpeed -= properties.gravity;
        else
            gravitySpeed = -controller.stepOffset / Time.deltaTime * 5;

        _desiredVelocity.y = gravitySpeed * Time.deltaTime;
        _controller.Move(_desiredVelocity * Time.deltaTime);
        Rotate(_desiredRotation);
    }


    //void PlayAnimationProperty(AnimationClipProperties clipProperties)
    //{
    //    _animatedGameObject.CrossFade(clipProperties.clip.name, clipProperties.blendTime);        
    //}

    //void UpdateAnimations()
    //{
    //    // This is pretty simple at the moment. Based on velocity (gravity cancelled) 
    //    // blend between idle walk and run animations 
    //    if (_isWithAnimatedObject && !_health.isDead)
    //    {
    //        // Use the real controller velocity, not the desired
    //        // this avoids "running constantly at a wall" effect
    //        Vector3 v = _controller.velocity;
    //        v.y = 0;
    //        if (v.magnitude < 0.05f) // <- hacky
    //        {
    //            _animatedGameObject.PlayQueued(animationProperties.idle.clip.name, QueueMode.CompleteOthers);
    //            //PlayAnimationProperty(animationProperties.idle);
    //            //_animatedGameObject.CrossFade(animationProperties.idle.clip.name, animationProperties.run.blendTime);
    //        }
    //        else
    //        {
    //            if ((v.magnitude / properties.speed.max) < animationProperties.walkToRunSpeedRatio)
    //                _animatedGameObject.CrossFade(animationProperties.walk.clip.name, animationProperties.walk.blendTime);
    //            else
    //                _animatedGameObject.CrossFade(animationProperties.run.clip.name, animationProperties.run.blendTime);
    //        }
    //    }
    //}


    void LateUpdate()
    {
        // Apply damping if no Brain input
        if (!_isBrainSetVelocityThisFrame && controller.isGrounded)
            _desiredVelocity = Vector3.MoveTowards(_desiredVelocity, Vector3.zero, properties.speed.damping * Time.deltaTime);
        _isBrainSetVelocityThisFrame = false;
    }

    

    private void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
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
        Vector3 v = _desiredVelocity;
        v.y = 0;

        // Blend between accelearation & damping based on dot of current/desired
        // this is so that if the desired direction is BEHIND you, you use damping values
        // to first STOP then Accelerate 
        float dotWithVel = (2 + Vector3.Dot(_controller.velocity.normalized, transform.forward)) * 0.5f;
        Vector3 targetVelocity = Vector3.Lerp(Vector3.zero, velocity, dotWithVel);
        v = Vector3.MoveTowards(v, targetVelocity, properties.speed.acceleration * Time.deltaTime);

        v = Vector3.ClampMagnitude(v, properties.speed.max);
        _desiredVelocity = v;

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
    public void SetDesiredVelocity(Vector3 velocity, Vector3 LookAtTarget)
    {
        CalculateDesiredVelocity(velocity);
        _desiredRotation = RotateToLookAt(LookAtTarget, properties.rotation.max);
    }


    public void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        //AnimationClipProperties pToPlay;

        //Vector3 dirToObject = Helpers.DirectionTo(transform, info.responsibleGameObject.transform);
        //if (_health.isDead)
        //{
        //    pToPlay = animationProperties.death;
        //    StartCoroutine(DieRotateRoutine(dirToObject));
        //}
        //else
        //{            
        //    Vector3 forward = transform.forward;
        //    Vector3 right = transform.right;
        //    dirToObject.y = forward.y = right.y = 0;
        //    float frontOrBack = Vector3.Dot(forward, dirToObject);
        //    float rightOrLeft = Vector3.Dot(right, dirToObject);


        //    // Must be front or back
        //    if (Mathf.Abs(frontOrBack) > 0.707f)
        //    {
        //        if (frontOrBack > 0)
        //            pToPlay = animationProperties.hitFromFront;
        //        else
        //            pToPlay = animationProperties.hitFromBehind;
        //    }
        //    else
        //    {
        //        // Must be left or right
        //        if (rightOrLeft > 0)
        //            pToPlay = animationProperties.hitFromRight;
        //        else
        //            pToPlay = animationProperties.hitFromLeft;
        //    }

        //    //Debug.DrawRay(transform.position, (info.responsibleGameObject.transform.position - transform.position), Color.red, 10f);
        //}
        //PlayAnimationProperty(pToPlay);
    }


    // Just quick hack to rotate toward last hit dir
    IEnumerator DieRotateRoutine(Vector3 directionLastHit)
    {
        directionLastHit.y = 0;
        directionLastHit.Normalize();
        Quaternion rotTarget = Quaternion.LookRotation(directionLastHit, Vector3.up);        

        float t = 0;
        float speed = 8f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            _desiredRotation = Quaternion.Lerp(transform.rotation, rotTarget, Time.deltaTime * speed );
            yield return null;
        }
    }
        


    //void OnDrawGizmos()
    //{
    //    if (!Application.isPlaying)
    //    {
    //        if (animationProperties == null)
    //            return;

    //        if (animationProperties.gizmoMesh == null)
    //            return;

    //        Color c = animationProperties.gizmoColor;
    //        c.a = 0.5f;
    //        Gizmos.color = c;
    //        Gizmos.DrawMesh(animationProperties.gizmoMesh, transform.position, transform.rotation);
    //    }
    //}
}
