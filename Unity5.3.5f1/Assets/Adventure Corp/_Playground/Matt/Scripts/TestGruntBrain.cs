using UnityEngine;
using System.Collections;

public class TestGruntBrain : Brain
{
    //Inputs
    Vector3 directionalInput;
    //    

    public AttackController atkController;

    Vector3 camDiff;

    protected override void Awake()
    {
        camDiff = Camera.main.transform.position - transform.position;
        base.Awake();
    }


    protected override void Update()
    {
        Camera.main.transform.position = transform.position + camDiff;
        GrabInput();
        UpdateState();
        base.Update();
    }

    public Vector3 GrabMovementDirRelativeToCam(Vector3 v)
    {
        return MathLab.ConvertVectorRelativeToCam(v, false);
    }

    void GrabInput()
    {
        directionalInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        directionalInput = GrabMovementDirRelativeToCam(directionalInput);

        // If movment, then break the attack
        if (directionalInput.magnitude > 0.05f)
            if(atkController.isAttacking)
                atkController.YieldControlFromAttack();

        if (Input.GetButtonDown("Fire1"))
        {
            atkController.AttackWithDescriptor(atkController.attacks[3]);
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
