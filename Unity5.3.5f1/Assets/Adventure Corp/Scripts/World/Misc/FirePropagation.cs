using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirePropagation : MonoBehaviour
{

	public bool isOnFire = false;
	public float radius = 0.5f;

	public float timeToCatchFire = 1f;

	public static List<FirePropagation> volumes = new List<FirePropagation>();

	Material material;

	void Awake()
	{
		material = GetComponent<Renderer>().materials[0];
		if (isOnFire)
			material.SetColor("_EmissionColor", Color.red);
		volumes.Add(this);
		StartCoroutine(UpdateRoutine());
	}


	public void CatchFire()
	{
		if (isOnFire)
			return;

		isOnFire = true;
		StartCoroutine(CatchFireRoutine());
	}

	IEnumerator CatchFireRoutine()
	{
		float t = 0;
		Color c = new Color(1f, 0.45f, 0f);
		while (t < timeToCatchFire)
		{
			t += Time.deltaTime;
			material.SetColor("_EmissionColor", Color.Lerp(Color.black, c, t / timeToCatchFire));
			yield return null;
		}
		material.SetColor("_EmissionColor", Color.red);
	}


	IEnumerator UpdateRoutine()
	{
		while (true)
		{
			if (isOnFire)
			{
				for (int i = 0; i < volumes.Count; i++)
					if (!volumes[i].isOnFire)
						if (Helpers.InRadius(transform.position, volumes[i].transform.position, radius + volumes[i].radius))
							volumes[i].CatchFire();
				yield return null;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	//void OnDrawGizmos()
	//{
	//	Gizmos.color = isOnFire ? Color.red : Color.blue;
	//	Gizmos.DrawWireSphere(transform.position, radius);
	//}
}
