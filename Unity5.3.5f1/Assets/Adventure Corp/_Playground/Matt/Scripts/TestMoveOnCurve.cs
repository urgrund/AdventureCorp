using UnityEngine;
using System.Collections;

public class TestMoveOnCurve : MonoBehaviour {

    public AnimationCurve cZ = new AnimationCurve();
    public AnimationCurve cY = new AnimationCurve();

    CharacterController c;

	// Use this for initialization
	void Start () {
        c = GetComponent<CharacterController>();
    }
	


	// Update is called once per frame
	IEnumerator UpdateR () {

        float t = 0;
        while (t < 1)
        {            
            Vector3 m = new Vector3();

            t = Mathf.Clamp01(t);
            m.z = cZ.Evaluate(t + Time.deltaTime) - cZ.Evaluate(t);
            m.y = cY.Evaluate(t + Time.deltaTime) -cY.Evaluate(t);

            c.Move(m);

            t += Time.deltaTime;
            yield return null;
        }
	}

    void OnGUI()
    {
        if (GUILayout.Button("Go"))
        {
            StopAllCoroutines();
            StartCoroutine(UpdateR());
        }
    }
}
