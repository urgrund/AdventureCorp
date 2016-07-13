using UnityEngine;
using System.Collections;

public class TestPlayableBrain : Brain
{
    //Inputs
    Vector3 directionalInput;
    Vector3 lookInput;
    //    

    AttackController atkController;
    public AttackDescriptor basicMelee;
    public AttackDescriptor basicRanged;

    //public LineRenderer lr;

    Vector3 camDiff;

    protected override void Awake()
    {
        camDiff = Camera.main.transform.position - transform.position;
        base.Awake();
    }

    protected override void Start()
    {        
        atkController = GetComponent<AttackController>();
        atkController.SetOwnerHealthToDamageVolumes(agent.health);

        foreach (Damager d in atkController.damagers)
            d.onDamageTriggerEnter += OnDamageTriggerEvent;

        lastLootAtTime = Time.time;

        base.Start();
    }


    protected override void Update()
    {
        Camera.main.transform.position = transform.position + camDiff;
        GrabInput();
        UpdateState();
        UpdateRangedFocusing();
        base.Update();
    }

    public Vector3 GrabMovementDirRelativeToCam(Vector3 v)
    {
        return MathLab.ConvertVectorRelativeToCam(v, false);
    }


    float lastLootAtTime;
    float lookAtTimeInterval = 0.25f;
    void OnDamageTriggerEvent(Damager damager, Health health)
    {
        //return;
        //if ((Time.time - lastLootAtTime) > lookAtTimeInterval)
        //{
        //    agent.SetRotation(health.transform);            
        //    lastLootAtTime = Time.time;
        //}
    }


    void LookAtNearestHealth()
    {
        // Slow approach, this is just to 
        // get some results to play with 

        // This approach simply gets the "most in front" agent
        // We can look at other approach such as "closest & in front"
        // though hopefully the OVerlapSphere is enough for distance

        // this is pretty generous, allows for 270deg if someone nearby        
        float biggestDotProduct = -0.707f;
        Health mostInFrontHealth = null;
        foreach (Collider c in Physics.OverlapSphere(transform.position, 5f))
        {
            Health h = c.GetComponent<Health>();
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
                    //Debug.Log(h.name + "    " + d);
                }
            }
        }

        if (mostInFrontHealth != null)
            agent.SetRotation(mostInFrontHealth.transform);
    }


    public float rangedFocusSpeed = 1f;
    public float rangedStartAngle = 45f;
    public float rangedDeFocusSpeed = 3f;
    public float rangedCurrentAngle = 45;
    void UpdateRangedFocusing()
    {
    }



    void GrabInput()
    {
        directionalInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        directionalInput = GrabMovementDirRelativeToCam(directionalInput);

        //lr.SetPosition(0, transform.position);
        //lr.SetPosition(1, transform.position + lookInput * 5);

        // If movment, then break the attack
        if (directionalInput.magnitude > 0.05f)
            if(atkController.isAttacking)
                atkController.YieldControlFromAttack();

        if (Input.GetButtonDown("Fire1"))
        {
            LookAtNearestHealth();
            atkController.AttackWithDescriptor(basicMelee);
        }
        else if (Input.GetButtonDown("Fire2"))
        {
            atkController.AttackWithDescriptor(basicRanged);
        }
    }


    void UpdateState()
    {
        //Idle
        if (directionalInput.magnitude != 0)
        {
            if(!atkController.isControllingAgentVelocity)
                agent.SetDesiredVelocity(directionalInput * agent.properties.speed.max, true);
        }
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawRay(transform.position, transform.forward * 5);
    //}
}
