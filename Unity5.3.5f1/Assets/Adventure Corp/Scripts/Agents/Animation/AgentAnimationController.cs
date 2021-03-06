﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// The animation controller is responsible for the visual 
/// representation of an agent.  Here, the animated model 
/// is spawned and animation playback is managed
/// </summary>
public class AgentAnimationController : MonoBehaviour
{
	public bool showDebugGUI = false;
	public AgentAnimationProperties animationProperties;

	private Agent agent;
	private bool _isAgentAttached = false;


	public string upperBodyTransformBoneName = "Character1_Spine";
	Transform _upperBodyTransform;

	/// <summary>
	/// Locomotion such as walking and idle will be taken care of automatically
	/// </summary>
	bool isAutoHandleBasicLocomotion = true;


	// -----  Animation State -----------------

	public enum State
	{
		Dead = 0,           //
		Idle = 1,           //
		Walking = 2,        //
		Running = 3,        //
		StrafeLeft = 4,
		StrafeRight = 5,
		BackPedal = 6,
		Override = 7        // Animation state is overridden 
	}

	private Dictionary<State, AnimationClipProperties> _locomotionClipDictionary;

	public State _state = State.Idle;
	public State state
	{
		get { return _state; }
		set { SetAnimationState(value); }
	}

	// -----  Animation State -----------------



	//[HideInInspector]
	public float overrideCountDown = 0;

	private Animation _animatedGameObject;
	public Animation animatedGameObject { get { return _animatedGameObject; } }


	List<Material> _materials;


	// ----------------------------------------
	void Awake()
	{
		if (animationProperties != null)
		{
			if (animationProperties.animatedGameObject != null)
			{
				_animatedGameObject = Helpers.InstantiateAndParent(animationProperties.animatedGameObject.gameObject.transform, transform, true).GetComponent<Animation>();
				_upperBodyTransform = Helpers.SearchHierarchyForTransform(_animatedGameObject.transform, upperBodyTransformBoneName);

				// Setup Dictionary
				// Map animation states to locomotion animation properties 
				_locomotionClipDictionary = new Dictionary<State, AnimationClipProperties>();
				_locomotionClipDictionary.Add(State.Dead, animationProperties.reaction.death);
				_locomotionClipDictionary.Add(State.Idle, animationProperties.locomotion.idle);
				_locomotionClipDictionary.Add(State.Walking, animationProperties.locomotion.walk);
				_locomotionClipDictionary.Add(State.StrafeLeft, animationProperties.locomotion.strafeLeft);
				_locomotionClipDictionary.Add(State.Running, animationProperties.locomotion.run);
				_locomotionClipDictionary.Add(State.StrafeRight, animationProperties.locomotion.strafeRight);
				_locomotionClipDictionary.Add(State.BackPedal, animationProperties.locomotion.backPedal);

				// Collect all materials on this animated model
				_materials = new List<Material>();
				foreach (SkinnedMeshRenderer s in _animatedGameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
					foreach (Material m in s.materials)
						_materials.Add(m);
			}
		}
		else
			Debug.LogError(name.ToString() + " has no animation properties.");
	}


	// ----------------------------------------
	void Start()
	{
		SetControllingAgent(GetComponent<Agent>());

		if (_isAgentAttached && isAutoHandleBasicLocomotion)
			StartCoroutine(BasicLocomotionRoutine());

		PrewarmAnimations();
		_state = State.Dead;
		SetAnimationState(State.Idle);
	}


	// ----------------------------------------
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


	// ----------------------------------------
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
	IEnumerator BasicLocomotionRoutine()
	{
		while (true)
		{
			if (_state != State.Override && state != State.Dead)
			{
				// Determine walk/run speed
				if (agent.speedRatio < 0.05f)
				{
					state = State.Idle;
				}
				else
				{
					switch (agent.moveDirection)
					{
						case Agent.MoveDirection.Foward:
							if (agent.speedRatio < agent.properties.walkToRunSpeedRatio)
								state = State.Walking;
							else
								state = State.Running;
							break;

						case Agent.MoveDirection.Back:
							state = State.BackPedal;
							break;

						case Agent.MoveDirection.Left:
							state = State.StrafeLeft;
							break;

						case Agent.MoveDirection.Right:
							state = State.StrafeRight;
							break;
					}
				}

				float speedRatio = Mathf.Clamp(agent.speedRatio, 0.25f, 1f);
				if (_state == State.Idle)
					speedRatio = 1f;
				animatedGameObject[_locomotionClipDictionary[state].clip.name].speed = _locomotionClipDictionary[state].playSpeed * speedRatio;
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

		if (_state != State.Override)
			if (_locomotionClipDictionary[state] != null)
				Play(_locomotionClipDictionary[state]);
	}



	void Update()
	{
		if (overrideCountDown > 0)
			overrideCountDown -= Time.deltaTime;

		if (_state == State.Override && overrideCountDown < 0)
		{
			overrideCountDown = 0f;
			if (state != State.Dead)
				state = State.Idle;
		}

		UpdateFlashMaterials();
	}

	bool isAlreadyDead = false;

	public void Play(AnimationClipProperties clipProperties) { Play(clipProperties, 0f, false); }
	public void Play(AnimationClipProperties clipProperties, bool immediateBlend) { Play(clipProperties, 0f, immediateBlend); }
	public void Play(AnimationClipProperties clipProperties, float offsetRatio) { Play(clipProperties, offsetRatio, false); }
	public void Play(AnimationClipProperties clipProperties, float offsetRatio, bool immediateBlend)
	{
		
		if (clipProperties == null)
		{
			Debug.LogError("Clip Properties NULL!!");
			return;
		}

		if (clipProperties.clip == null)
		{
			Debug.LogError("No clip on animation property on {0}  " + this.name, clipProperties);
			return;
		}

		if (_animatedGameObject[clipProperties.clip.name] == null)
		{
			Debug.LogError("This clip doesn't exist on the animated object! (" + clipProperties.clip.name + ")");
			return;
		}

		if (state == State.Dead)
		{
			if (isAlreadyDead)
				return;

			_animatedGameObject.Stop();
			_animatedGameObject.clip = _locomotionClipDictionary[state].clip;
			_animatedGameObject.Play(_locomotionClipDictionary[state].clip.name, PlayMode.StopAll);
			isAlreadyDead = true;
			return;
		}

		_animatedGameObject[clipProperties.clip.name].speed = clipProperties.playSpeed;
		_animatedGameObject[clipProperties.clip.name].blendMode = clipProperties.blendMode;
		_animatedGameObject[clipProperties.clip.name].layer = (int)clipProperties.layer;
		_animatedGameObject[clipProperties.clip.name].enabled = true;
		float clipBlendTime = immediateBlend ? 0f : clipProperties.blendTime;

		if (clipProperties.isMixingTransform)
			_animatedGameObject[clipProperties.clip.name].AddMixingTransform(_upperBodyTransform);

		if (clipProperties.isOverriding)
		{
			_animatedGameObject.Rewind(clipProperties.clip.name);
			//_animatedGameObject.clip = clipProperties.clip;
			if(_stopAnimRoutine != null)
				StopCoroutine(_stopAnimRoutine);
			_animatedGameObject[clipProperties.clip.name].weight = clipProperties.weight;
			_animatedGameObject[clipProperties.clip.name].normalizedTime = offsetRatio;
			_animatedGameObject.CrossFade(clipProperties.clip.name, clipBlendTime);

			overrideCountDown = clipProperties.scaledLength;
			state = State.Override;
		}
		else
		{
			if (clipProperties.isMixingTransform)
			{
				_animatedGameObject.Rewind(clipProperties.clip.name);
				_animatedGameObject[clipProperties.clip.name].normalizedTime = offsetRatio;
				_animatedGameObject.Blend(clipProperties.clip.name, clipProperties.weight, clipBlendTime);
			}
			else
			{
				_animatedGameObject[clipProperties.clip.name].normalizedTime = offsetRatio;
				_animatedGameObject.CrossFade(clipProperties.clip.name, clipBlendTime);
			}
		}

		// 
		if (clipProperties.clip.wrapMode == WrapMode.ClampForever && !clipProperties.isOverriding)
			Stop(clipProperties, false, clipProperties.scaledLength);
	}


	public void Stop(AnimationClipProperties clipProperties, bool useBlendTime = false, float delay = 0)
	{
		if (useBlendTime || delay > 0)
		{
			//StartCoroutine(StopAnimationOverTimeRoutine(clipProperties, useBlendTime, delay));
			_stopAnimRoutine = StopAnimationOverTimeRoutine(clipProperties, useBlendTime, delay);
			StartCoroutine(_stopAnimRoutine);
		}
		else
		{
			_animatedGameObject[clipProperties.clip.name].weight = 0f;
			_animatedGameObject.Stop(clipProperties.clip.name);
		}
	}

	IEnumerator _stopAnimRoutine = null;
	IEnumerator StopAnimationOverTimeRoutine(AnimationClipProperties clipProperties, bool useBlendTime, float delay)
	{
		if (delay > 0)
			yield return new WaitForSeconds(delay);

		if (!useBlendTime)
			Stop(clipProperties, false);

		float t = 0;
		while (t < clipProperties.blendTime)
		{
			t += Time.deltaTime;
			_animatedGameObject[clipProperties.clip.name].weight = 1f - (t / clipProperties.blendTime);
			yield return null;
		}
		Stop(clipProperties, false);
	}



	// ----------------------------------------------------------------------------
	// Flashes materials red
	// This is a bit sloppy here perhaps.   Will
	// need a specific character shader anyway that has flash
	// perameters among other things
	private float _flashValue = 0;
	private Color _flashColor = Color.red;
	private float _flashFadeSpeed = 5;
	private string _emissionProperty = "_EmissionColor";
	private void UpdateFlashMaterials()
	{
		_flashValue -= Time.deltaTime * _flashFadeSpeed;
		_flashValue = Mathf.Clamp01(_flashValue);
		foreach (Material m in _materials)
			m.SetColor(_emissionProperty, Color.Lerp(Color.black, _flashColor, _flashValue));
	}

	public void FlashMaterials(Color color, float speed = 5f)
	{
		_flashValue = 1f;
		_flashColor = color;
		_flashFadeSpeed = speed;
	}
	// ----------------------------------------------------------------------------




	public void OnHealthLost(Health.HealthChangedEventInfo info)
    {
        if (state == State.Dead)
            return;

		// set this always for visual feedback
		FlashMaterials(Color.red);
        

        // Determine direction the hit came from
        // and play the appropriate 'hitfrom' animation
        AnimationClipProperties pToPlay;
        Vector3 dirToObject = Helpers.DirectionTo(transform, info.responsibleGameObject.transform);
        if (agent.health.isDead)
        {            
            state = State.Dead;
        }
        else
        {
			// If staggered, let the staggere process complete
			if (agent.isStaggered)
			{
				return;
			}

			Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            dirToObject.y = forward.y = right.y = 0;
            float frontOrBack = Vector3.Dot(forward, dirToObject);
            float rightOrLeft = Vector3.Dot(right, dirToObject);

            // Must be front or back
            if (Mathf.Abs(frontOrBack) > 0.707f)
            {
                if (frontOrBack > 0)
                    pToPlay = animationProperties.reaction.hitFromFront;
                else
                    pToPlay = animationProperties.reaction.hitFromBehind;
            }
            else
            {
                // Must be left or right
                if (rightOrLeft > 0)
                    pToPlay = animationProperties.reaction.hitFromRight;
                else
                    pToPlay = animationProperties.reaction.hitFromLeft;
            }

            Play(pToPlay);            
        }
    }


    void OnGUI()
    {
        if (!showDebugGUI)
            return;

        if (animatedGameObject == null)
            return;

        float nameWidth = 180;
        float miscW = 60;        
        float boxHeight = 20;
        //Color bg = GUI.backgroundColor;
        if (Application.isPlaying)
        {
            foreach (AnimationState anim in animatedGameObject)
            {
                GUILayout.BeginHorizontal();

                // Name
                GUI.color = Color.white;
                GUILayout.Label("   " + anim.clip.name, GUILayout.Width(nameWidth));

                //
                GUI.color = Color.white;
                GUILayout.Label("Layer " + anim.layer, GUILayout.Width(miscW));
                GUILayout.Label(anim.blendMode.ToString(), GUILayout.Width(miscW));
                GUILayout.Label(anim.weight.ToString(), GUILayout.Width(miscW));

                GUI.color = Color.Lerp(Color.red, Color.green, anim.weight);
                GUI.backgroundColor = GUI.color;
                GUILayout.TextField("", GUILayout.Width(anim.weight * 100), GUILayout.MaxHeight(boxHeight));                

                GUILayout.EndHorizontal();
            }
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
