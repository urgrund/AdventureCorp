using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;


public class PatrolZone : MonoBehaviour
{

#if UNITY_EDITOR
	[MenuItem("GameObject/Adventure Corp/Patrol Zone", false, 10)]
	static void CreateCustomGameObject(MenuCommand menuCommand)
	{
		// Create a custom game object
		GameObject go = new GameObject("p_PatrolZone");
		go.AddComponent<PatrolZone>();
		go.isStatic = true;
		
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
#endif



	private List<PatrolPoint> _patrolPoints = new List<PatrolPoint>();
	private List<PatrolZone> _connectedPatrolZones = new List<PatrolZone>();
	public List<PatrolPoint> patrolPoints { get { return _patrolPoints; } }
	public List<PatrolZone> connectedPatrolZones { get { return _connectedPatrolZones; } }

	public void AddConnectedPatrolZone(PatrolZone zone)
	{
		if (zone != null)
			if (zone != this)
				if (!_connectedPatrolZones.Contains(zone))
					_connectedPatrolZones.Add(zone);
	}

	public void RemoveConnectedPatrolZone(PatrolZone zone)
	{
		if (zone != null)
			if (zone != this)
				if (_connectedPatrolZones.Contains(zone))
					_connectedPatrolZones.Remove(zone);
	}

	public void ClearAllConnectedPatrolZones()
	{
		foreach (PatrolZone p in _connectedPatrolZones)
			p.RemoveConnectedPatrolZone(this);
		_connectedPatrolZones.Clear();
	}


	void Awake()
	{		
		PatrolManager.Register(this);
		_patrolPoints = GeneratePatrolPointsForZone();
	}
	
	public PatrolPoint GetAvailablePatrolPoint()
	{
		List<PatrolPoint> avail = _patrolPoints.FindAll(x => !x.isBusy);
		if (avail.Count > 0)
			return avail[Random.Range((int)0, (int)avail.Count)];
		else
			return null;
	}

	public bool hasConnectedZones { get { return _connectedPatrolZones.Count > 0; } }

	public PatrolZone GetRandomConnectedZone()
	{
		if(hasConnectedZones)
			return Helpers.ListShuffle(_connectedPatrolZones)[0];
		return null;
	}
	
	private float rayUp = 5;
	private List<PatrolPoint> GeneratePatrolPointsForZone()
	{
		float spacing = AdventureCorpGlobals.Editor.Measurements.patrolPointSpacing;
		int xPoints = Mathf.FloorToInt(transform.localScale.x / spacing);
		int yPoints = Mathf.FloorToInt(transform.localScale.z / spacing);

		Vector3 offset = (Vector3.one / 2f) - new Vector3(0.5f / xPoints, 0, 0.5f / yPoints);
		offset.y = 0;

		
		List<PatrolPoint> points = new List<PatrolPoint>();
		for (int i = 0; i < xPoints; i++)
		{
			for (int j = 0; j < yPoints; j++)
			{
				Vector3 newPosition = (new Vector3((float)i / xPoints, 0, (float)j / yPoints)) - offset;
				newPosition = transform.TransformPoint(newPosition);
				PatrolPoint pp = new PatrolPoint(newPosition, this);					
				points.Add(pp);
			}
		}
		return points;
	}
	




	void OnDrawGizmos() { DrawGizmos(false); }
	void OnDrawGizmosSelected() { DrawGizmos(true); }
	void DrawGizmos(bool isSelected)
	{
		Color c;
		float alpha = isSelected ? 0.75f : 0.02f;
		

		if (isSelected)
		{	
			if(!Application.isPlaying)		
				_patrolPoints = GeneratePatrolPointsForZone();

			foreach (PatrolPoint p in _patrolPoints)
			{
				Gizmos.color = p.isBusy ? Color.red : Color.blue;
				Gizmos.DrawWireCube((p.position), Vector3.one * (p.isBusy ? 0.5f : 0.25f));
			}
		}

		// Gizmo handle 
		Gizmos.color = Color.blue;
		float h = 1f;
		Vector3 size = new Vector3(h * 0.2f, h, h * 0.2f);
		Vector3 offset = Vector3.up * h * 0.5f;
		Gizmos.DrawCube(transform.position + offset, size);
		Gizmos.DrawSphere(transform.position + Vector3.up * h, 0.3f);


		Matrix4x4 m = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, 0, 1));
		c = Color.cyan;
		c.a = alpha * 0.25f;
		Gizmos.color = c;
		Gizmos.DrawCube(Vector3.zero, new Vector3(1, 0, 1));
		Gizmos.matrix = m;



		// Draw links to other patrol zones
		c = Color.yellow;
		c.a = alpha;
		Gizmos.color = c;
		if (_connectedPatrolZones.Count > 0)
		{
			for (int i = 0; i < _connectedPatrolZones.Count; i++)
			{
				NavMeshPath path = new NavMeshPath();
				NavMesh.CalculatePath(transform.position, _connectedPatrolZones[i].transform.position, NavMesh.AllAreas, path);
				if (path != null)
				{
					for (int j = 0; j < path.corners.Length - 1; j++)
					{
						Gizmos.DrawLine(path.corners[j], path.corners[j + 1]);
						Gizmos.DrawSphere(path.corners[j], 0.25f);
					}
				}
			}
		}
	}
}


















/*

GameObject o;
public void GeneratePatrolPointsInCircle()
{
	if (patrolPoints.Count > 0)
	{
		patrolPoints.Clear();
	}

	var children = new List<GameObject>();
	foreach (Transform child in transform) children.Add(child.gameObject);
	children.ForEach(child => DestroyImmediate(child)); // Destroy all current patrol points if exiting

	if (numberOfPoints == 0)
		Debug.LogError("Number of points should not be zero");
	else //Create patrol points in a circle given an offset distance
	{
		float div = (1 / (float)numberOfPoints) * Mathf.PI * 2;
		for (int i = 0; i < numberOfPoints; i++)
		{
			Vector3 p = new Vector3(Mathf.Sin(div * (i + 1)), 0, Mathf.Cos(div * (i + 1))).normalized * offsetDistance + transform.position;
			p.y = transform.position.y;
			o = new GameObject("Patrol Point " + i.ToString());
			o.transform.position = p;
			if (Helpers.IsPointOutsideNavMesh(o.transform.position)) //Sample patrol point if outside navmesh
			{
				NavMeshHit hit;
				if (NavMesh.SamplePosition(o.transform.position, out hit, 10, NavMesh.AllAreas))
				{
					o.transform.position = hit.position;
				}
			}
			o.transform.parent = transform;
			PatrolPoint point = o.AddComponent<PatrolPoint>();
			patrolPoints.Add(point);
		}
	}

}




public void GeneratePatrolPointsRandomly()
{
	if (patrolPoints.Count > 0)
	{
		patrolPoints.Clear();
	}

	var children = new List<GameObject>();
	foreach (Transform child in transform) children.Add(child.gameObject);
	children.ForEach(child => DestroyImmediate(child)); // Destroy all current patrol points if exiting

	if (numberOfPoints == 0)
		Debug.LogError("Number of points should not be zero");
	else
	{
		//Create patrol points randomly in circle and sample them on navmesh
		for (int i = 0; i < numberOfPoints; i++)
		{
			Vector2 randomPoint = Random.insideUnitCircle * zoneRaduis;
			Vector3 p = new Vector3(randomPoint.x, transform.position.y, randomPoint.y) + transform.position;
			p.y = transform.position.y;
			o = new GameObject("Patrol Point " + i.ToString());
			o.transform.position = p;
			if (Helpers.IsPointOutsideNavMesh(o.transform.position))
			{
				NavMeshHit hit;
				if (NavMesh.SamplePosition(o.transform.position, out hit, 10, NavMesh.AllAreas))
				{
					o.transform.position = hit.position;
				}
			}
			o.transform.parent = transform;
			PatrolPoint point = o.AddComponent<PatrolPoint>();
			patrolPoints.Add(point);
		}
	}
}

public void SamplePatrolPointsOnNavMesh()
{
	var children = new List<GameObject>();
	foreach (Transform child in transform)
	{
		if (Helpers.IsPointOutsideNavMesh(child.transform.position))
		{
			NavMeshHit hit;
			if (NavMesh.SamplePosition(child.transform.position, out hit, 10, NavMesh.AllAreas))
			{
				child.transform.position = hit.position;
			}
		}
	}
}

public bool IsPatrolAreaFree()
{
	int count = 0;
	for (int i = 0; i < patrolPoints.Count; i++)
	{
		if (!patrolPoints[i].isBusy)
			count++;

		if (count >= 2)
			return true;
	}
	return false;
}

public PatrolPoint GrabRandomFreePatrolPoint()
{
	List<PatrolPoint> nonBusyPoints = new List<PatrolPoint>();
	for (int i = 0; i < patrolPoints.Count; i++)
	{
		if (!patrolPoints[i].isBusy)
		{
			nonBusyPoints.Add(patrolPoints[i]);
		}
	}

	if (nonBusyPoints.Count == 0)
		return null;
	else
	{
		int randomIndex = Random.Range((int)0, (int)nonBusyPoints.Count);
		return nonBusyPoints[randomIndex];
	}
}
*/