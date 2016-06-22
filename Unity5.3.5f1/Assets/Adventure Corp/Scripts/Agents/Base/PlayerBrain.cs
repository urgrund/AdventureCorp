using UnityEngine;
using System.Collections;

public class PlayerBrain : Brain
{
    //Inputs
    Vector3 directionalInput;
    //
	
	protected override void Update ()
    {
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
    }

    void UpdateState()
    {
        //Idle
        if(directionalInput.magnitude == 0)
        {
            SetVelocity(Vector3.zero);
        }
        //Walk
        else
        {
            SetVelocity(directionalInput * agent.speed);
            if (currentVelocity != Vector3.zero)
                currentRotation = CalculateRotationRelativeToVelocity(agent.rotationSpeed);
        }
       
    }
}
