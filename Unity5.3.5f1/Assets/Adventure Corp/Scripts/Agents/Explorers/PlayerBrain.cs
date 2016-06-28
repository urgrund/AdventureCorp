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
            agent.SetDesiredVelocity(Vector3.zero,true);
        }
        //Walk
        else
        {
            agent.SetDesiredVelocity(directionalInput * agent.properties.speed.max, true);
        }       
    }
}
