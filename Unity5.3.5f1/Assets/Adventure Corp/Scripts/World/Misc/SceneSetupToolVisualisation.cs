using UnityEngine;
using System.Collections;

public class SceneSetupToolVisualisation : MonoBehaviour
{
	void Awake()
	{
		Debug.LogError("Visualization Dummy Object was in scene at Run Time!");
		Destroy(this.gameObject);
	}

	Color colliderWireColor = Color.red;
	public float colliderColorAlpha = 0.25f;


	public BoxCollider[] allBoxes;
	public SphereCollider[] allSpheres;
	public bool isDrawColliders;

	void OnDrawGizmos()
	{
		if (isDrawColliders)
		{
			if (allBoxes != null)
			{
				foreach (BoxCollider c in allBoxes)
				{
					Gizmos.matrix = c.transform.localToWorldMatrix;					
					Gizmos.color = colliderWireColor;
					Gizmos.DrawWireCube(c.center, c.size);
					Gizmos.color = colliderWireColor * new Color(1,1,1,colliderColorAlpha);
					Gizmos.DrawCube(c.center, c.size);
				}
			}
		}
	}
}
