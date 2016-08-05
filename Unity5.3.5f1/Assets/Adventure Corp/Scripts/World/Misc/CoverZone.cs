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

	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Helpers.Draw.GizmoRing(transform.position, size);
	}
}
