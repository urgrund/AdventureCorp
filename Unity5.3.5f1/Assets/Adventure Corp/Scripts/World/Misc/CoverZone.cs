using UnityEngine;
using System.Collections;

public class CoverZone : MonoBehaviour
{
	public enum Shape
	{
		Square,
		Circle
	}

	public float size = 1f;
	public Shape shape = Shape.Square;

	void Reset()
	{
		Bounds b = Helpers.GetBoundsOfCompoundObject(this.gameObject);
		size = b.size.magnitude * 0.5f;
	}


	// TODO 
	// Use the code I wrote for the 'coin spacing' 
	// that had rings and straight lines to dynamically 
	// space out cover points
	private float spacing = 1f;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Helpers.Draw.GizmoRing(transform.position + Vector3.up, size);
	}
}
