using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Base abstract level Brain for playable Agents
/// manages input with Rewired 
/// </summary>
public abstract class ExplorerBrain : Brain
{
	// ------   INPUT -----------------------------

	/// <summary>
	/// ID for player controllers
	/// </summary>
	[HideInInspector]
	public int ID = 0;

    /// <summary>
    /// The Rewired player that maps to device input
    /// </summary>
    protected Rewired.Player _player;
    public Rewired.Player player { get { return _player; } }
	protected Vector3 _inputDirection;	
    protected Vector3 _inputAim;
	// --------------------------------------------


	public delegate void OnExplorerEvent();
	//public OnExplorerEvent onParrySuccessful;
	//public OnExplorerEvent onParryShieldBroken;
	//public OnExplorerEvent onParryShieldPelletLost;
	public OnExplorerEvent onRangedReady;

	


	/// <summary>
	/// The profile object of this explorer
	/// </summary>
	public ExplorerProfile profile;
	public ExplorerAttackCollection attackCollection;
	public AnimationClipProperties shieldUpClipProperties;
	protected AttackController _attackController;



	protected override void Awake()
    {
		Debug.Log(profile.explorerName + " entered the dungeon!");
        _player = Rewired.ReInput.players.GetPlayer(ID);
		ResetRanged();
		SetupShield();
		base.Awake();
    }

    protected override void Start()
    {
        Debug.Assert(GetComponent<AttackController>() != null, "No Attack Controller on Player");
		_attackController = GetComponent<AttackController>();
		_attackController.SetOwnerHealthToDamageVolumes(agent.health);
        base.Start();
    }

    protected override void Update()
    {
		// TODO - don't have this hacky test here
		if (!CameraSystem.instance)
		{
			Vector3 offset = new Vector3(-5, 8, -5);
			Camera.main.transform.position = transform.position + offset;
		}

		if (agent.isStaggered)
			return;

		UpdateShield();
		UpdateRanged();
		ProcessInput();		
		UpdateAgent(); 
		
        base.Update();
    }




	protected Vector3 GrabMovementDirRelativeToCam(Vector3 v) { return MathLab.ConvertVectorRelativeToCam(v, false); }



	public LineRenderer lr;




	// --------------------------------------------------------------------------
	// ----   Shield/Parry test stuff ----	
	public Transform shieldPellet;
	float shieldLastHoldTime;
	float shieldParryTimeWindow = 0.4f;
	List<Transform> shieldPellets = new List<Transform>();

	// These can stay here until we have proper
	// pellet assets with materials defining color
	Color parryPelletColorFull = Color.blue;
	Color parryPelletColorDefault = Color.cyan;
	int parryPelletCount;


	private void SetupShield()
	{
		parryPelletCount = profile.statistics.parryPellets;
		shieldLastHoldTime = -1f;
		float maxDeg = 15f;
		float frac = (maxDeg * 2) / (parryPelletCount - 1);
		for (int i = 0; i < parryPelletCount; i++)
		{
			Transform t = Helpers.InstantiateAndParent(shieldPellet, this.transform, true);
			t.Rotate(new Vector3(0, 0, maxDeg - (frac * (i))), Space.Self);
			shieldPellets.Add(t);
		}

		AdjustShieldParryPellets();
		StartCoroutine(ParryPelletRegenRoutine());
	}


	private void AdjustShieldParryPellets()
	{
		parryPelletCount = Mathf.Clamp(parryPelletCount, 0, profile.statistics.parryPellets);

		Color c;
		if (parryPelletCount == profile.statistics.parryPellets)
			c = parryPelletColorFull;
		else
			c = parryPelletColorDefault;


		foreach (Transform t in shieldPellets)
			t.gameObject.SetActive(false);

		for (int i = 0; i < parryPelletCount; i++)
		{
			shieldPellets[i].gameObject.SetActive(true);
			shieldPellets[i].GetComponentInChildren<MeshRenderer>().material.color = c;
		}
	}



	private IEnumerator ParryPelletRegenRoutine()
	{
		while (true)
		{
			if (parryPelletCount < profile.statistics.parryPellets
				&& !agent.isStaggered
				&& !agent.health.isDead)
			{
				yield return new WaitForSeconds(profile.statistics.parryPelletReGenInterval);
				parryPelletCount++;
				AdjustShieldParryPellets();
			}
			yield return null;
		}
	}

	public bool isShieldUpAndProtecting { get { return player.GetButton(AdventureCorpGlobals.Input.Player.SHIELD) && parryPelletCount > 0; } }

	// --------------------------------------------------------------------------








	// --------------------  RANGED  -------------------------------------

	protected void ResetRanged()
	{
		_isRangedCharged = false;
		_rangedCurrentAngle = profile.statistics.rangedStartAngle;
	}

	void RangedLineRenderUpdate()
	{
		lr.enabled = (_inputAim.magnitude > 0.01f);
			
		
		Vector3 p = Vector3.zero;
		// Below is for the line renderer visuals
		if (lr == null || !lr.enabled)
			return;

		float d = 5f;
		p = new Vector3(Mathf.Sin(Mathf.Deg2Rad * _rangedCurrentAngle), 0, Mathf.Cos(Mathf.Deg2Rad * _rangedCurrentAngle));
		lr.SetPosition(0, p * d + Vector3.up);
		lr.SetPosition(1, Vector3.up);
		p.x = -p.x;
		lr.SetPosition(2, p * d + Vector3.up);

		Color c = _rangedCurrentAngle < Mathf.Epsilon ? Color.green : Color.blue;
		c.a = 0.15f;
		lr.material.color = c;
	}

	protected float _rangedCurrentAngle = 45;
	protected bool _isRangedCharged = false;
	void UpdateRanged()
	{		
		if (_inputAim.magnitude > 0.01f)
		{
			agent.SetDesiredRotation(_inputAim);
			_rangedCurrentAngle -= Time.deltaTime * (profile.statistics.rangedStartAngle/profile.statistics.rangedTimeToFocus);
			_rangedCurrentAngle = Mathf.Clamp(_rangedCurrentAngle, 0, profile.statistics.rangedStartAngle);		
			if (_rangedCurrentAngle < Mathf.Epsilon && !_isRangedCharged)
			{
				_isRangedCharged = true;
			}
		}
		else
			ResetRanged();

		// Temp for now
		RangedLineRenderUpdate();
	}

	// ------------------------------------------------------------------------










	// --------------------  SHIELD / PARRY  -------------------------------------

	void UpdateShield()
	{
		// HACK - just set health to invincible whilst holding 
		// shield,  this is probably not what we want, but just to see some results
		agent.health.invincible = isShieldUpAndProtecting;


		if (player.GetButtonDown(AdventureCorpGlobals.Input.Player.SHIELD))
			shieldLastHoldTime = Time.time;

		if (player.GetButton(AdventureCorpGlobals.Input.Player.SHIELD) && !agent.health.isDead && !agent.isStaggered)
		{
			agent.animationController.Play(shieldUpClipProperties, true);
			agent.SetVelocityScaleThisFrame(profile.statistics.parryBlockMovementScale);
			LookAtNearestHealthComponent(5f);
		}
		else
		{
			agent.animationController.Stop(shieldUpClipProperties, true);
		}
	}

	protected override void OnHealthWasInvincible(Health.HealthChangedEventInfo info)
	{
		if (player.GetButton(AdventureCorpGlobals.Input.Player.SHIELD))
			agent.SetDesiredRotation(info.responsibleGameObject.transform, true);


		// Parry is based on the responsible object 
		// having a valid attack controller 
		if (info.responsibleAttackController != null)
		{
			// Parry based on how soon before the first damage is dealt
			// by an agent...  this keeps the timing consistent and based on 
			// animation times/lengths rather than 
			float realStartTimeForDamage = info.responsibleAttackController.currentAttackStartTime + info.responsibleAttackController.currentAttackValidDamageRangeStartTime;
			float shieldHoldTime = Time.time - shieldLastHoldTime;


			// Successful Parry!!!
			// Player timed the shield hold at the right time 
			// triggering a parry/counter attack 
			if (info.responsibleAttackController.currentAttack.canAttackBeParried &&
				(realStartTimeForDamage - shieldLastHoldTime) <= shieldParryTimeWindow)
			{
				_attackController.AttackWithDescriptor(attackCollection.parry);

				Agent a = info.responsibleGameObject.GetComponent<Agent>();
				if (a)
				{
					agent.animationController.FlashMaterials(Color.green, 3f);
					a.OverrideMove(Helpers.DirectionTo(transform, info.responsibleGameObject.transform));
					a.Stagger();
				}

				// Test idea - push camera in a little
				Vector3 p = Camera.main.transform.position + Camera.main.transform.forward;
				Camera.main.transform.position = p;
			}
			else
			{
				// Absorbed hit but no parry
				// This wil reduce pellet count and push back the explorer
				// based on the pushback values
				if (parryPelletCount > 0)
				{
					parryPelletCount--;
					AdjustShieldParryPellets();

					agent.animationController.FlashMaterials(Color.blue, 2f);

					float pushBack = agent.properties.pushBackVelocityScale;
					if (info.responsibleAttackController != null)
						pushBack *= info.responsibleAttackController.currentAttack.pushBackScale;

					// Temp idea to reduce push back if blocking
					pushBack *= 0.66f;

					agent.OverrideMove(-Helpers.DirectionTo(transform, info.responsibleGameObject.transform) * pushBack);
				}
				// No pellets and was trying to block, so this is a stagger
				// to the player explorer and should take the damage that was dealt
				else
				{
					agent.health.invincible = false;
					agent.health.TakeDamage(info.damage, info.responsibleGameObject);
					agent.Stagger();
				}
			}
		}

		base.OnHealthWasInvincible(info);
	}




	// To manage being staggered
	float _lastHitTime = 0f;
	float _dmgCount = 0;
	protected override void OnAgentStaggered()
	{
		agent.animationController.FlashMaterials(Color.yellow, 1);
		base.OnAgentStaggered();
	}

	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if ((Time.time - _lastHitTime) < profile.statistics.staggerDmgReceiveWindow)
		{
			_dmgCount += info.value;
			if (_dmgCount > profile.statistics.staggerDmgThreshold)			
				agent.Stagger();			
		}
		else
		{
			_lastHitTime = info.time;
			_dmgCount = info.value;
		}
		
		base.OnHealthLost(info);
	}

	protected override void OnHealthZero(Health.HealthChangedEventInfo info)
	{
		StartCoroutine(HackDeathRestartLevel());
		base.OnHealthZero(info);
	}

	IEnumerator HackDeathRestartLevel()
	{
		yield return new WaitForSeconds(2f);
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
	}

	// ------------------------------------------------------------------------











	/// <summary>
	/// This approach simply gets the "most in front" agent
	/// We can look at other approach such as "closest & in front"
	/// though hopefully the OVerlapSphere is enough for distance
	/// </summary>
	protected void LookAtNearestHealthComponent(float range = 2.5f)
    {            
        float biggestDotProduct = -0.707f; // cos(-45deg)
        Health mostInFrontHealth = null;
        Collider[] c = Physics.OverlapSphere(transform.position, range);
        for (int i = 0; i < c.Length; i++)
        {
            Health h = c[i].GetComponent<Health>();
            if (h && !h.isDead)
            {
                if (h != agent.health)
                {
                    float d = Vector3.Dot(Helpers.DirectionTo(transform, h.transform), transform.forward);
                    if (d > biggestDotProduct)
                    {
                        mostInFrontHealth = h;
                        biggestDotProduct = d;
                    }
                }
            }
        }

        if (mostInFrontHealth != null)
            agent.SetDesiredRotation(mostInFrontHealth.transform);
    }




    protected void AttackNowAsMelee()
    {
		if (agent.isGrounded)
		{
			LookAtNearestHealthComponent();		
			_attackController.AttackWithDescriptor(attackCollection.GetNextMeleeFromSequence(_attackController));
		}
    }



    protected void AttackNowAsRanged(AttackDescriptor attack)
    {
		if (agent.isGrounded && _isRangedCharged)
		{			
			_attackController.AttackWithDescriptor(attack);
			ResetRanged();
		}
    }




	// ------------------------------------------------------------------------
	// INPUT 
	private float _inputThreshold = 0.01f;
	private float _lastDashTime = 0f;
    protected void ProcessInput()
    {
		// TODO - need to manage controller 'time outs' if they log off after being left idle

		// Movement direction
		_inputDirection = new Vector3(player.GetAxis(AdventureCorpGlobals.Input.Player.MOVE_HORIZONTAL), 0, player.GetAxis(AdventureCorpGlobals.Input.Player.MOVE_VERTICAL));
        _inputDirection = GrabMovementDirRelativeToCam(_inputDirection);

		// Aiming Direction
		_inputAim = new Vector3(player.GetAxis(AdventureCorpGlobals.Input.Player.AIM_HORIZONTAL),0, player.GetAxis(AdventureCorpGlobals.Input.Player.AIM_VERTICAL));		
		_inputAim = GrabMovementDirRelativeToCam(_inputAim);


		// Dash
		if (player.GetButtonDown(AdventureCorpGlobals.Input.Player.DASH))
		{
			if ((Time.time - _lastDashTime) > profile.statistics.dashCoolDown)
			{
				_lastDashTime = Time.time;
				if (_inputDirection.magnitude > _inputThreshold)
					agent.SetDesiredRotation(_inputDirection.normalized, true);
				_attackController.AttackWithDescriptor(attackCollection.dash);
				return;
			}
		}

		// If movment, then break the attack
		if (_inputDirection.magnitude > _inputThreshold)
			_attackController.YieldControlFromAttack();

        if (player.GetButtonDown(AdventureCorpGlobals.Input.Player.MELEE))
            AttackNowAsMelee();
        else if (player.GetButtonUp(AdventureCorpGlobals.Input.Player.RANGED))
            AttackNowAsRanged(attackCollection.ranged);
    }


    protected void UpdateAgent()
    {
        if (_inputDirection.magnitude > 0)
        {
			if (agent.isGrounded && !agent.isOverrideMoveThisFrame)
			{
				if (!_attackController.isControllingAgentVelocity)
				{
					if (_inputAim.magnitude > Mathf.Epsilon)
					{
						agent.SetDesiredRotation(_inputAim);
						agent.SetDesiredVelocity(_inputDirection * agent.properties.speed.max);
					}
					else
						agent.SetDesiredVelocity(_inputDirection * agent.properties.speed.max, true);
				}
				else
				{
					if (_attackController.isAttacking && !_attackController.isPastTurnToTime)
						agent.SetDesiredRotation(_inputDirection);
				}
			}
        }
    }
	// ------------------------------------------------------------------------



	void OnGUI()
	{
		int healthPellets = agent.health.maxHealth / 10;

		GUILayout.BeginHorizontal();
		GUILayout.Label("     -");
		for (int i = 0; i < healthPellets; i++)
		{
			GUI.color = agent.health.currentHealth > (i * 10) ? Color.green : Color.red;
			GUILayout.Label("♥");
		}
		GUILayout.EndHorizontal();
	}




	void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 5);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, _inputAim.normalized * 5);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, _inputDirection.normalized * 5);
    }
}
