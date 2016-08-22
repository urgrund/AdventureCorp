using UnityEngine;
using System.Collections;

public class TestSizeUp : NPCBrain
{
   
    float A = 0;
    float B = 0;
    float C = 0;
    float D = 0;
    float amplitude = 1.5f;
    float t = 1000;
	protected override IEnumerator Update_AttackState()
    {        

        Vector3 sizeUpPos = transform.position;
        _desiredMoveSpeed = agent.properties.speed.max * 0.15f;
        isLookAtPlayer = true;
        t = 1000;
        A = Random.Range(0.4f, 1.25f);
        B = Random.Range(0.4f, 1.25f);
        C = Random.Range(0.4f, 1.25f);
        D = Random.Range(0.4f, 1.25f);

        while (true)
        {
            float x = A * MathLab.CosWave(amplitude, A, 0, Time.time) + B * MathLab.SinWave(amplitude, B, 0, Time.time) + C * MathLab.CosWave(amplitude, C, 0, Time.time) + D * MathLab.SinWave(amplitude, D, 0, Time.time);
            float z = A * MathLab.SinWave(amplitude, A, 0, Time.time) + B * MathLab.CosWave(amplitude, B, 0, Time.time) + C * MathLab.SinWave(amplitude, C, 0, Time.time) + D * MathLab.SinWave(amplitude, D, 0, Time.time);
            Vector3 pos = new Vector3(x, 0, z) + sizeUpPos;
            destination = pos;
            Helpers.Draw.Marker(pos, 2, Color.red, 0, true);
            t += Time.deltaTime;
            yield return null;
        }

     //   _desiredMoveSpeed = agent.properties.speed.max;
        //isLookAtPlayer = false;
        
    }
}