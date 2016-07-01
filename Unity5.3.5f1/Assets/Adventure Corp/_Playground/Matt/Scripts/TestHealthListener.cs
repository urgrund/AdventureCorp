using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class TestHealthListener : MonoBehaviour
{
    Health h;

    // Use this for initialization
    void Start()
    {
        h = GetComponent<Health>();
        h.onHealthLost += OnLostHealth;
        h.onHealthZero += OnDead;
        StartCoroutine(LerpToGrey());
    }


    void OnLostHealth(int i)
    {
        print("Ouch!");
        t = 0;
    }

    void OnDead(int i)
    {
        print("Dead");
    }

    float t = 1;
    IEnumerator LerpToGrey()
    {
        while (true)
        {
            t += Time.deltaTime * 5f;
            t = Mathf.Clamp01(t);
            if (GetComponent<Renderer>())
            {
                Material m = GetComponent<Renderer>().material;
                m.color = Color.Lerp(Color.red, Color.grey, t);
            }
            yield return null;
        }        
    }

}
