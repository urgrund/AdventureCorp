using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TestPlayableExplorer : ExplorerBrain
{


	// --------------------------------------------------------------------------
	// ----   Shield/Parry test stuff ----

	public Transform shield;
	public Transform shieldPellet;

    float shieldMovementScale = 0.25f;
	Material shieldMaterial;									
    float shieldFlashCoolDownSpeed = 4f;
    float shieldFlashCurrent = 0;
    Color shieldColorHit = Color.blue;
    Color shieldColorParry = Color.green;
    Color shieldColorFlashCurrent = Color.black;
    float shieldLastHoldTime;
    float shieldParryTimeWindow = 0.4f;
	List<Transform> shieldPellets = new List<Transform>();

	Color parryPelletColorFull = Color.blue;
	Color parryPelletColorDefault = Color.cyan;
	int parryPelletCount;

	void SetupShield()
	{
		parryPelletCount = profile.statistics.parryPellets;

		shield = Helpers.InstantiateAndParent(shield, transform, true);
		shield.gameObject.SetActive(false);
		shieldMaterial = shield.GetComponentInChildren<MeshRenderer>().material;
		shieldLastHoldTime = -1f;

		float maxDeg = 15f;		

		float frac = (maxDeg * 2) / (parryPelletCount-1);

		for (int i = 0; i < parryPelletCount; i++)
		{
			Transform t = Helpers.InstantiateAndParent(shieldPellet, shield, true);
			t.Rotate(new Vector3(0, 0, maxDeg - (frac * (i))), Space.Self);
			shieldPellets.Add(t);
		}
		

		AdjustShieldParryPellets();
		StartCoroutine(ParryPelletRegenRoutine());
	}


	void AdjustShieldParryPellets()
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



	IEnumerator ParryPelletRegenRoutine()
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

	public bool isShieldUpAndProtecting { get { return player.GetButton(INPUT_SHIELD) && parryPelletCount > 0; } }


	// --------------------------------------------------------------------------


	public LineRenderer lr;



    protected override void Awake()
    {
		SetupShield();		
        base.Awake();
    }


    


    protected override void Update()
	{
//#if UNITY_EDITOR
		if (!CameraSystem.instance)
		{
			Vector3 offset = new Vector3(-5, 8, -5);
			Camera.main.transform.position = transform.position + offset;
		}
//#endif
		base.Update();		
		if (!agent.isStaggered)
		{
			UpdateRangedFocusing();
			UpdateShieldStuff();
		}
    }


    // --------------------  SHIELD / PARRY TEST -------------------------------------
            
    void UpdateShieldStuff()
    {
		// Manages color of the shield for visual feedback
        shieldFlashCurrent -= Time.deltaTime * shieldFlashCoolDownSpeed;
        shieldFlashCurrent = Mathf.Clamp01(shieldFlashCurrent);
        shieldMaterial.SetColor("_EmissionColor", Color.Lerp(Color.black, shieldColorFlashCurrent, shieldFlashCurrent));

		// HACK - just set health to invincible whilst holding 
		// shield,  this is probably not what we want, but just to see some results
		agent.health.invincible = isShieldUpAndProtecting;


		if (player.GetButtonDown(INPUT_SHIELD))
            shieldLastHoldTime = Time.time;

        if (player.GetButton(INPUT_SHIELD) && !agent.health.isDead)
        {
            shield.gameObject.SetActive(true);
            agent.SetVelocityScaleThisFrame(shieldMovementScale);			
			LookAtNearestHealthComponent(5f);    
        }
        else
            shield.gameObject.SetActive(false);
    }

    protected override void OnHealthWasInvincible(Health.HealthChangedEventInfo info)
    {
        shieldFlashCurrent = 1f;
        if (player.GetButton(INPUT_SHIELD))
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

			// Player timed the shield hold at the right time 
			// triggering a parry/counter attack 
			if (info.responsibleAttackController.currentAttack.canAttackBeParried &&
				(realStartTimeForDamage - shieldLastHoldTime) <= shieldParryTimeWindow)
			{
				shieldColorFlashCurrent = shieldColorParry;
				AttackNowAsMelee(attackCollection.melee1);

				// Stagger and pushback on successful parry                                 
				Agent a = info.responsibleGameObject.GetComponent<Agent>();
				if (a)
				{
					a.OverrideMove(Helpers.DirectionTo(transform, info.responsibleGameObject.transform) * 1f);
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
					shieldColorFlashCurrent = shieldColorHit;

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
					shield.gameObject.SetActive(false);
					agent.health.invincible = false;
					agent.health.TakeDamage(info.damage, info.responsibleGameObject);
					print("foo");
					agent.Stagger();
				}
			}
		}

        base.OnHealthWasInvincible(info);
    }

	public float lastHitTime = 0f;
	public float staggerWindow = 1f;
	public float staggerDmg = 30;
	public float dmgCount = 0;

	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if ((Time.time - lastHitTime) < staggerWindow)
		{
			dmgCount += info.value;
			if (dmgCount > staggerDmg)
			{				
				agent.Stagger();
			}
		}
		else
		{			
			lastHitTime = info.time;
			dmgCount = info.value;
		}

		//Debug.Log("Player lost health - " + info.value + "   at : " + Time.realtimeSinceStartup);
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









	// --------------------  RANGED FOCUS TEST -------------------------------------

	float rangedFocusSpeed = 60f;
    float rangedStartAngle = 45f;
    float rangedDeFocusSpeed = 100f;
    float rangedCurrentAngle = 45;
    void UpdateRangedFocusing()
    {
        _inputAim.x = player.GetAxis(INPUT_AIM_HORIZONTAL);
        _inputAim.z = player.GetAxis(INPUT_AIM_VERTICAL);
        _inputAim = GrabMovementDirRelativeToCam(_inputAim);

        Vector3 p = Vector3.zero;
        if (_inputAim.magnitude > 0.01f)
        {
            agent.SetDesiredRotation(_inputAim);
            rangedCurrentAngle -= Time.deltaTime * rangedFocusSpeed;
            float x = Mathf.Sin(Mathf.Deg2Rad * rangedCurrentAngle);
            float z = Mathf.Cos(Mathf.Deg2Rad * rangedCurrentAngle);
            p = new Vector3(x, 0, z);
        }
        else
        {
			rangedCurrentAngle = rangedStartAngle;// Time.deltaTime * rangedDeFocusSpeed;
        }

        if (lr == null)
            return;

        float d = 5f;
        lr.SetPosition(0, p * d + Vector3.up);
        lr.SetPosition(1, Vector3.up);
        p.x = -p.x;
        lr.SetPosition(2, p * d + Vector3.up);

        rangedCurrentAngle = Mathf.Clamp(rangedCurrentAngle, 0f, rangedStartAngle);

        Color c = rangedCurrentAngle < 1f ? Color.green : Color.blue;
        lr.material.color = c;
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

}
