using UnityEngine;
using System.Collections;


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
    public int ID = 0;

    /// <summary>
    /// The Rewired player that maps to device input
    /// </summary>
    protected Rewired.Player _player;
    public Rewired.Player player { get { return _player; } }

    // Static variables for input mapping
    protected static readonly string INPUT_MELEE = "Melee";
    protected static readonly string INPUT_RANGED = "Ranged";
    protected static readonly string INPUT_SHIELD = "Shield";
    protected static readonly string INPUT_DASH = "Dash";
    protected static readonly string INPUT_MOVE_HORIZONTAL = "Move Horizontal";
    protected static readonly string INPUT_MOVE_VERTICAL = "Move Vertical";
    protected static readonly string INPUT_AIM_HORIZONTAL = "Aim Horizontal";
    protected static readonly string INPUT_AIM_VERTICAL = "Aim Vertical";

    protected Vector3 _inputDirection;
    protected Vector3 _inputAim;
    // --------------------------------------------
	




    /// <summary>
    /// The profile object of this explorer
    /// </summary>
    public ExplorerProfile profile;

	/// <summary>
	/// Object to hold all attack information
	/// </summary>
	public BaseAttackCollection attackCollection;




    protected override void Awake()
    {
		Debug.Log(profile.explorerName + " entered the game!");
        _player = Rewired.ReInput.players.GetPlayer(ID);
        base.Awake();
    }

    protected override void Start()
    {
        Debug.Assert(GetComponent<AttackController>() != null, "No Attack Controller on Player");
        attackCollection.controller = GetComponent<AttackController>();
        attackCollection.controller.SetOwnerHealthToDamageVolumes(agent.health);
        base.Start();
    }

    protected override void Update()
    {
		if (agent.isStaggered)
			return;

        ProcessInput();
        UpdateAgent();
        base.Update();
    }




    protected Vector3 GrabMovementDirRelativeToCam(Vector3 v)
    {
        return MathLab.ConvertVectorRelativeToCam(v, false);
    }


    /// <summary>
    /// This approach simply gets the "most in front" agent
    /// We can look at other approach such as "closest & in front"
    /// though hopefully the OVerlapSphere is enough for distance
    /// </summary>
    void LookAtNearestHealthComponent()
    {            
        float biggestDotProduct = -0.707f; // -45deg
        Health mostInFrontHealth = null;
        Collider[] c = Physics.OverlapSphere(transform.position, 2.5f);
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




    protected void AttackNowAsMelee(AttackDescriptor attack)
    {
		if (agent.isGrounded)
		{
			LookAtNearestHealthComponent();
			attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);
		}
    }

    protected void AttackNowAsRanged(AttackDescriptor attack)
    {
		if (agent.isGrounded)
		{
			attackCollection.controller.AttackWithDescriptor(attackCollection.ranged1);
		}
    }

	private float _inputThreshold = 0.01f;
	private float _lastDashTime = 0f;
    protected void ProcessInput()
    {
        _inputDirection = new Vector3(player.GetAxis(INPUT_MOVE_HORIZONTAL), 0, player.GetAxis(INPUT_MOVE_VERTICAL));
        _inputDirection = GrabMovementDirRelativeToCam(_inputDirection);

		// Dash
		if (player.GetButtonDown(INPUT_DASH))
		{
			if ((Time.time - _lastDashTime) > profile.statistics.dashCoolDown)
			{
				_lastDashTime = Time.time;
				if (_inputDirection.magnitude > _inputThreshold)
				{
					agent.SetDesiredRotation(_inputDirection.normalized, true);
					agent.OverrideMove(_inputDirection.normalized * Time.deltaTime * profile.statistics.dashSpeed, profile.statistics.dashDuration );
					return;
				}
			}
		}


		// If movment, then break the attack
		if (_inputDirection.magnitude > _inputThreshold)
            if (attackCollection.controller.isAttacking)
                attackCollection.controller.YieldControlFromAttack();

        if (player.GetButtonDown(INPUT_MELEE))
            AttackNowAsMelee(attackCollection.melee1);
        else if (player.GetButtonDown(INPUT_RANGED))
            AttackNowAsRanged(attackCollection.ranged1);
    }


    protected void UpdateAgent()
    {
        if (_inputDirection.magnitude > 0)
        {
			if (!attackCollection.controller.isControllingAgentVelocity
				&& agent.isGrounded
				&& !agent.isOverrideMoveThisFrame)
			{
				agent.SetDesiredVelocity(_inputDirection * agent.properties.speed.max, true);
			}
        }
    }

	void OnGUI()
	{
		if (GUILayout.Button("Stagger"))
			agent.Stagger();
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
