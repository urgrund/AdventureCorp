using UnityEngine;
using System.Collections;


/// <summary>
/// Base abstract level Brain for playable Agents
/// manages input with Rewired 
/// </summary>
public abstract class PlayerBrain : Brain
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
    protected static string INPUT_MELEE = "Melee";
    protected static string INPUT_RANGED = "Ranged";
    protected static string INPUT_MOVE_HORIZONTAL = "Move Horizontal";
    protected static string INPUT_MOVE_VERTICAL = "Move Vertical";
    protected static string INPUT_AIM_HORIZONTAL = "Aim Horizontal";
    protected static string INPUT_AIM_VERTICAL = "Aim Vertical";

    protected Vector3 _inputDirection;
    protected Vector3 _inputAim;


    // --------------------------------------------



    protected AttackController atkController;
    public AttackDescriptor basicMelee;
    public AttackDescriptor basicRanged;



    protected override void Awake()
    {
        _player = Rewired.ReInput.players.GetPlayer(ID);
        base.Awake();
    }

    protected override void Start()
    {
        Debug.Assert(atkController == null, "No Attack Controller on Player");
        atkController = GetComponent<AttackController>();
        atkController.SetOwnerHealthToDamageVolumes(agent.health);

        base.Start();
    }

    protected override void Update()
    {
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


    protected void ProcessInput()
    {
        _inputDirection = new Vector3(player.GetAxis(INPUT_MOVE_HORIZONTAL), 0, player.GetAxis(INPUT_MOVE_VERTICAL));
        _inputDirection = GrabMovementDirRelativeToCam(_inputDirection);


        // If movment, then break the attack
        if (_inputDirection.magnitude > 0.05f)
            if (atkController.isAttacking)
                atkController.YieldControlFromAttack();

        if (player.GetButtonDown(INPUT_MELEE))
        {
            LookAtNearestHealthComponent();
            atkController.AttackWithDescriptor(basicMelee);
        }
        else if (player.GetButtonDown(INPUT_RANGED))
        {
            atkController.AttackWithDescriptor(basicRanged);
        }
    }


    protected void UpdateAgent()
    {
        if (_inputDirection.magnitude > 0)
        {
            if (!atkController.isControllingAgentVelocity)
                agent.SetDesiredVelocity(_inputDirection * agent.properties.speed.max, true);
        }
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
