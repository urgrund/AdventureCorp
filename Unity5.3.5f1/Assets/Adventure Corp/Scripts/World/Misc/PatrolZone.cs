using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PatrolZone : MonoBehaviour
{
    [HideInInspector]
    public bool isBusy = false;
    public float radius = 10f;
    public float offsetDistance = 8;
    public int numberOfPoints = 8;
    [HideInInspector]
    public List<PatrolPoint> patrolPoints = new List<PatrolPoint>();

    public List<PatrolZone> connectedPatrolAreas = new List<PatrolZone>();

    void Awake()
    {
        if(patrolPoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PatrolPoint>())
                    patrolPoints.Add(child.GetComponent<PatrolPoint>());
            }
        }
    }

    void Start()
    {
        if (PatrolManager.instance)
            PatrolManager.Register(this);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Helpers.GizmoDrawRing(transform.position, radius);

        float h = 1f;
        Vector3 size = new Vector3(h * 0.2f, h, h * 0.2f);
        Vector3 offset = Vector3.up * h * 0.5f;
        Gizmos.DrawCube(transform.position + offset, size);
        Gizmos.DrawSphere(transform.position + Vector3.up * h, 0.3f);


		Color c = Color.yellow;
		c.a = 0.2f;
        Gizmos.color = c;
        if(connectedPatrolAreas.Count > 0)
        {
            for(int i = 0; i < connectedPatrolAreas.Count; i++)
            {
               NavMeshPath path = new NavMeshPath();
               NavMesh.CalculatePath(transform.position, connectedPatrolAreas[i].transform.position,NavMesh.AllAreas, path);
               if(path != null)
               {
                    for(int j = 0; j <  path.corners.Length - 1; j++)
                    {
                        Gizmos.DrawLine(path.corners[j], path.corners[j + 1]);
                        Helpers.GizmoDrawRing(path.corners[j], 1.0f);
                    }
               }
            }
        }
    }

    GameObject o;
    public void GeneratePatrolPointsInCircle()
    {
        if(patrolPoints.Count > 0)
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
                if(Helpers.IsPointOutsideNavMesh(o.transform.position)) //Sample patrol point if outside navmesh
                {
                    NavMeshHit hit;
                    if(NavMesh.SamplePosition(o.transform.position,out hit, 10, NavMesh.AllAreas))
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
                Vector2 randomPoint = Random.insideUnitCircle * radius;
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
        for(int i =0; i< patrolPoints.Count; i++)
        {
            if(!patrolPoints[i].isBusy)
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
}
