using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class Agent : MonoBehaviour
{    
    private Brain _brain;

    private CharacterController _controller;
    public CharacterController controller {  get { return _controller; } }

    
    public AgentProperties properties;
    public AgentAnimationProperties animationProperties;
    private Animation _animatedGameObject;
    public Animation animatedGameObject { get { return _animatedGameObject; } }
    
    
    private float gravitySpeed = 0f;

    [HideInInspector]
    public Vector3 currentVelocity;
    [HideInInspector]
    public Quaternion currentRotation;

    private bool _isWithAnimatedObject = false;

    private bool _isBrainSetVelocityThisFrame = false;

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

    void Awake ()
    {
        if (animationProperties != null)
        {
            if (animationProperties.animatedGameObject != null)
            {
                _animatedGameObject = Helpers.InstantiateAndParent(animationProperties.animatedGameObject.gameObject.transform, transform, true).GetComponent<Animation>();
                _isWithAnimatedObject = true;
            }
        }
        else
            Debug.LogError(name.ToString() + " has no animation properties.");

        //brain = GetComponent<Brain>();
        _controller = GetComponent<CharacterController>();

        //if (brain)
          //  PlugBrain(brain);
        //else
          //  Debug.LogError("There is no Brain attached on me, how am I supposed to function?");
    }

    void Update()
    {
        UpdateController();
        UpdateAnimations();        
    }


    void UpdateController()
    {
        // Apply gravity
        if (!controller.isGrounded)
            gravitySpeed -= properties.gravity;
        else
            gravitySpeed = -controller.stepOffset / Time.deltaTime * 5;

        currentVelocity.y = gravitySpeed * Time.deltaTime;
        _controller.Move(currentVelocity * Time.deltaTime);
        Rotate(currentRotation);
    }


    void UpdateAnimations()
    {        
        // This is pretty simple at the moment. Based on velocity (gravity cancelled) 
        // blend between idle walk and run animations 
        if (_isWithAnimatedObject)
        {
            // Use the real controller velocity, not the desired
            // this avoids "running constantly at a wall" effect
            Vector3 v = _controller.velocity;
            v.y = 0;
            if (v.magnitude < 0.05f) // <- hacky
            {
                _animatedGameObject.CrossFade(animationProperties.idle.clip.name, animationProperties.run.blendTime);
            }
            else
            {
                if ((v.magnitude / properties.speed.max) < animationProperties.walkToRunSpeedRatio)
                    _animatedGameObject.CrossFade(animationProperties.walk.clip.name, animationProperties.walk.blendTime);
                else
                    _animatedGameObject.CrossFade(animationProperties.run.clip.name, animationProperties.run.blendTime);
            }
        }
    }


    void LateUpdate()
    {
        // Apply damping if no Brain input
        if (!_isBrainSetVelocityThisFrame && controller.isGrounded)
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, properties.speed.damping * Time.deltaTime);
        _isBrainSetVelocityThisFrame = false;
    }

	public void PlugBrain(Brain brain)
    {
        this._brain = brain;        
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
        Vector3 v = currentVelocity;
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
        // such as ice.  This could simply be scalars to acceleartion/damping etc

        Vector3 v = currentVelocity;
        v.y = 0;

        // TODO - blend between accelearation & damping based on dot of current/desired
        // this is so that if the desired direction is BEHIND you, you use damping values
        // to first STOP then Accelerate 
        v = Vector3.MoveTowards(v, velocity, properties.speed.acceleration * Time.deltaTime);
        v = Vector3.ClampMagnitude(v, properties.speed.max);
        currentVelocity = v;

        _isBrainSetVelocityThisFrame = true;
    }

    /// <summary>
    /// Set player velocity and rotate the character towards velocity direction. If no rotation is needed set the bool isRotate to false
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, bool isRotate)
    {
        CalculateDesiredVelocity(velocity);

        if (currentVelocity != Vector3.zero && isRotate)
            currentRotation = RotateToVelocityDirection(properties.rotation.max);
    }

    /// <summary>
    /// Set player velocity and rotate the character towards a vector3 target
    /// </summary>
    public void SetDesiredVelocity(Vector3 velocity, Vector3 LookAtTarget)
    {
        CalculateDesiredVelocity(velocity);
        currentRotation= RotateToLookAt(LookAtTarget, properties.rotation.max);
    }



    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (animationProperties == null)
                return;

            if (animationProperties.gizmoMesh == null)
                return;


            Color c = animationProperties.gizmoColor;
            c.a = 0.5f;
            Gizmos.color = c;
            Gizmos.DrawMesh(animationProperties.gizmoMesh, transform.position, transform.rotation);
        }
    }
}
