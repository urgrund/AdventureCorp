using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoverZone : MonoBehaviour
{
	public enum Shape
	{
		Square,
		Circle
	}


	public float size = 1f;
	public Shape shape = Shape.Square;

	float rayUp = 3;

	float _pointSpacing = 4.5f;
	private List<Vector3> _points = new List<Vector3>();
	public List<Vector3> points { get { return _points; } }

		
	void Reset()
	{
		Bounds b = Helpers.GetBoundsOfCompoundObject(this.gameObject);
		size = b.size.magnitude * 0.5f;
		PreparePoints();		
	}

	void PreparePoints()
	{
		_points.Clear();
		float circleSegment = Mathf.Deg2Rad * 360f;
		float segments = (circleSegment * size) / _pointSpacing;
		int pointCount = Mathf.FloorToInt(segments) + 1;
		float circleFraction = (1 / (float)pointCount) * circleSegment;

		for (int i = 0; i < pointCount; i++)
		{
			Vector3 newPoint = GetPointOnRing(circleFraction * i);

			//NavMeshHit hit;
			//if (NavMesh.SamplePosition(newPoint, out hit, 10, NavMesh.AllAreas))			
			NavMeshHit hit;
			if (!NavMesh.Raycast(newPoint + Vector3.up * rayUp, newPoint - Vector3.down * rayUp, out hit, NavMesh.AllAreas))
			{
				newPoint.y = hit.position.y;
				_points.Add(newPoint);
			}
		}
	}

	Vector3 GetPointOnRing(float rad)
	{		
		Vector3 newPoint = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized * size;
		return newPoint + transform.position;
	}
	
	void DrawPoints()
	{
		Helpers.Draw.GizmoRing(transform.position + Vector3.up, size);

		if (_points != null)
		{
			foreach (Vector3 p in _points)
				Gizmos.DrawWireSphere(p, 0.15f);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow * new Color(1,1,1,0.15f);
		DrawPoints();
	}

	void OnDrawGizmosSelected()
	{
		PreparePoints();
		Gizmos.color = Color.yellow;
		DrawPoints();
	}
}
