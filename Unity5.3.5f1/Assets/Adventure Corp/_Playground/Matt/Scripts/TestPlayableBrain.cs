using UnityEngine;
using System.Collections;

// Matt - we can use this namespace, but I feel it makes our code a little
// ambiguous with "player" now a class.  Having to use Rewired.Player helps
// with the readability there
//using Rewired;

public class TestPlayableBrain : PlayerBrain
{ 
    public LineRenderer lr;

  
    protected override void Update()
    {           
        base.Update();
        UpdateRangedFocusing();
    }

    
      

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
            rangedCurrentAngle += Time.deltaTime * rangedDeFocusSpeed;
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

}
