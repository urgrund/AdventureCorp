using UnityEngine;
using System.Collections;



public class AgentAnimationController : MonoBehaviour
{
    public AgentAnimationProperties animationProperties;

    public string mixingTransformBoneName = "root";
    private Transform mixingTransformBone;

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
    }

    void Awake()
    {
        if (animationProperties != null)
        {
            if (animationProperties.animatedGameObject != null)
            {
                _animatedGameObject = Helpers.InstantiateAndParent(animationProperties.animatedGameObject.gameObject.transform, transform, true).GetComponent<Animation>();
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
        Play(animationProperties.idle);
    }
    
        

    public void PrewarmAnimations()
    {
        // Matt - got this code snippet from online
        // Animation.RebuildInternalState causes massive frame spikes
        // http://forum.unity3d.com/threads/animation-rebuildinternalstate-ms-spikes.123480/#post-1560774
        // This iterates through the animations to read them into memory in order to  try avoiding this.
        int animationCount = 0;        
        for (int i = 0; i < _animatedGameObject.GetClipCount(); i++)
        {
            animationCount++;
        }
    }


    /// <summary>
    /// Handles walking, running idle and dying
    /// Also will manage strafe and backpedalling based on agent 
    /// </summary>
    public float overrideCountDown = 0;
    IEnumerator BasicLocomotionRoutine()
    {
        state = State.Idle;
        while (true)
        {
            if (overrideCountDown <= 0f)
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

                // Depending on the state, adjust playback 
                if (_state == State.Running)
                    animatedGameObject[animationProperties.run.clip.name].speed = agent.speedRatio;
                if (_state == State.Walking)
                    animatedGameObject[animationProperties.walk.clip.name].speed = agent.speedRatio;
            }
            else
            {
                overrideCountDown -= Time.deltaTime;
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

    

    public void Play(AnimationClipProperties clipProperties)
    {
        if (clipProperties.clip == null)
            Debug.LogError("No clip on animation property on {0}", clipProperties);

        _animatedGameObject[clipProperties.clip.name].speed = clipProperties.playSpeed;
        _animatedGameObject[clipProperties.clip.name].weight = clipProperties.weight;
        _animatedGameObject[clipProperties.clip.name].blendMode = clipProperties.blendMode;

        if (clipProperties.isOverriding)
        {
            _animatedGameObject.Stop();
            _animatedGameObject.CrossFade(clipProperties.clip.name, clipProperties.blendTime); 
            overrideCountDown = clipProperties.clip.length;
            state = State.Override;
            //StopAllCoroutines();
            //breakHold = true;
            //StartCoroutine(HoldStateThenReturn(State.Other, State.Idle, clipProperties.clip.length));
        }
        else
        {
            _animatedGameObject[clipProperties.clip.name].layer = 2;
            _animatedGameObject.CrossFade(clipProperties.clip.name, clipProperties.blendTime);
            if (!clipProperties.clip.isLooping)
                overrideCountDown = clipProperties.clip.length;
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
