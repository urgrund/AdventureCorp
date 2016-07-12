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
            agent.SetDesiredVelocity(directionalInput * agent.properties.speed.max, true);
        }
    }
}
