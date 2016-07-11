using UnityEngine;
using System.Collections;



public class AgentAnimationController : MonoBehaviour
{
    //public bool showDebugGUI = false;
    public AgentAnimationProperties animationProperties;

    public string upperBodyTransformBoneName = "spine";
    Transform _upperBodyTransform;

    [Tooltip("This needs an Agent present and will take care of basic locomotion based on the Agents velocity and movement.")]
    public bool isAutoHandleBasicLocomotion = true;
    private Agent agent;

    bool _isAgentAttached = false;



    public enum State
    {
        Dead,           //
        Idle,           //
        Walking,        //
        Running,        //
        Override        // Animation state is overridden 
    }


    public State _state = State.Idle;
    public State state
    {
        get { return _state; }
        set { SetAnimationState(value); }
    }


    private Animation _animatedGameObject;
    public Animation animatedGameObject { get { return _animatedGameObject; } }

    public void SetControllingAgent(Agent agent)
    {
        this.agent = agent;
        _isAgentAttached = (this.agent != null);
        if (agent != null)
        {
            agent.health.onHealthLost += OnHealthLost;
            agent.health.onHealthZero += OnHealthLost;
        }
    }

    void Awake()
    {
        if (animationProperties != null)
        {
            if (animationProperties.animatedGameObject != null)
            {
                _animatedGameObject = Helpers.InstantiateAndParent(animationProperties.animatedGameObject.gameObject.transform, transform, true).GetComponent<Animation>();
                _upperBodyTransform = Helpers.SearchHierarchyForTransform(_animatedGameObject.transform, upperBodyTransformBoneName);
            }
        }
        else
            Debug.LogError(name.ToString() + " has no animation properties.");
    }

    void Start()
    {
        SetControllingAgent(GetComponent<Agent>());
                
        if (_isAgentAttached && isAutoHandleBasicLocomotion)
            StartCoroutine(BasicLocomotionRoutine());

        PrewarmAnimations();
        _state = State.Dead;
        SetAnimationState(State.Idle);

        //Play(animationProperties.idle);
    }
    
        

    public void PrewarmAnimations()
    {
        // Matt - got this code snippet from online
        // Animation.RebuildInternalState causes massive frame spikes
        // http://forum.unity3d.com/threads/animation-rebuildinternalstate-ms-spikes.123480/#post-1560774
        // This iterates through the animations to read them into memory in order to  try avoiding this.
        int animationCount = 0;        
        for (int i = 0; i < _animatedGameObject.GetClipCount(); i++)
            animationCount++;
    }


    /// <summary>
    /// Handles walking, running idle and dying
    /// Also will manage strafe and backpedalling based on agent 
    /// </summary>
    public float overrideCountDown = 0;
    IEnumerator BasicLocomotionRoutine()
    {
        while (true)
        {
            //if (overrideCountDown <= 0f)
            if(_state != State.Override && state != State.Dead)
            {
                // Determine walk/run speed
                if (agent.speedRatio < 0.05f)
                {
                    state = State.Idle;
                }
                else
                {
                    if (agent.speedRatio < animationProperties.walkToRunSpeedRatio)
                        state = State.Walking;
                    else
                        state = State.Running;
                }

                //Depending on the state, adjust playback
                float speedRatio = Mathf.Clamp(agent.speedRatio, 0.25f, 1f);
                if (_state == State.Running)
                    animatedGameObject[animationProperties.run.clip.name].speed = animationProperties.run.playSpeed * speedRatio;
                if (_state == State.Walking)
                    animatedGameObject[animationProperties.walk.clip.name].speed = animationProperties.walk.playSpeed * speedRatio;
            }
            yield return null;
        }  
    }


    void SetAnimationState(State value)
    {
        // if state is the same, just return to avoid
        // setting Animation component values
        if (value != _state)
            _state = value;
        else
            return;

        switch (_state)
        {
            case State.Dead:
                Play(animationProperties.death);
                break;

            case State.Idle:
                Play(animationProperties.idle);
                break;

            case State.Walking:
                Play(animationProperties.walk);
                break;

            case State.Running:
                Play(animationProperties.run);
                break;
        }
    }



    void Update()
    {
        if (overrideCountDown > 0)
            overrideCountDown -= Time.deltaTime;

        if (_state == State.Override && overrideCountDown < 0)
        {
            overrideCountDown = 0f;
            if(state != State.Dead)
                state = State.Idle;
        }
    }


    public void Play(AnimationClipProperties clipProperties)
    {
        if (state == State.Dead)
        {            
            _animatedGameObject.Stop();
            _animatedGameObject.Play(animationProperties.death.clip.name, PlayMode.StopAll);
            return;
        }


        if (clipProperties.clip == null)
            Debug.LogError("No clip on animation property on {0}", clipProperties);


        _animatedGameObject[clipProperties.clip.name].speed = clipProperties.playSpeed;
        _animatedGameObject[clipProperties.clip.name].weight = clipProperties.weight;
        _animatedGameObject[clipProperties.clip.name].blendMode = clipProperties.blendMode;
        _animatedGameObject[clipProperties.clip.name].layer = (int)clipProperties.layer;
        _animatedGameObject[clipProperties.clip.name].enabled = true;

        if (clipProperties.isMixingTransform)
            _animatedGameObject[clipProperties.clip.name].AddMixingTransform(_upperBodyTransform);


        if (clipProperties.isOverriding)
        {
            _animatedGameObject.Rewind(clipProperties.clip.name);
            _animatedGameObject.clip = clipProperties.clip;
            _animatedGameObject.CrossFade(clipProperties.clip.name, clipProperties.blendTime);

            overrideCountDown = clipProperties.clip.length * (1f / clipProperties.playSpeed);
            state = State.Override;
        }
        else
        {
            if (clipProperties.isMixingTransform)
            {
                _animatedGameObject.Rewind(clipProperties.clip.name);
                _animatedGameObject.Blend(clipProperties.clip.name, clipProperties.weight, clipProperties.blendTime);
            }
            else
                _animatedGameObject.CrossFade(clipProperties.clip.name, clipProperties.blendTime);
        }
    }


    bool breakHold = false;
    IEnumerator HoldStateThenReturn(State hold, State returnTo, float timeToWait)
    {
        print("Start");

        breakHold = false;
        state = hold;
        float t = 0;
        while (t < timeToWait)
        {
            t += Time.deltaTime;
            if (breakHold)
                state = returnTo;
            else
                yield return null;
        }
        state = returnTo;

        print("Stop");
    }

    public void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        if (state == State.Dead)
            return;


        AnimationClipProperties pToPlay;
        Vector3 dirToObject = Helpers.DirectionTo(transform, info.responsibleGameObject.transform);
        if (agent.health.isDead)
        {            
            state = State.Dead;
        }
        else
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            dirToObject.y = forward.y = right.y = 0;
            float frontOrBack = Vector3.Dot(forward, dirToObject);
            float rightOrLeft = Vector3.Dot(right, dirToObject);

            // Must be front or back
            if (Mathf.Abs(frontOrBack) > 0.707f)
            {
                if (frontOrBack > 0)
                    pToPlay = animationProperties.hitFromFront;
                else
                    pToPlay = animationProperties.hitFromBehind;
            }
            else
            {
                // Must be left or right
                if (rightOrLeft > 0)
                    pToPlay = animationProperties.hitFromRight;
                else
                    pToPlay = animationProperties.hitFromLeft;
            }

            Play(pToPlay);
            //Debug.DrawRay(transform.position, (info.responsibleGameObject.transform.position - transform.position), Color.red, 10f);
        }
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
