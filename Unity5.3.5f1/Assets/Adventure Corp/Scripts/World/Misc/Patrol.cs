using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Patrol : MonoBehaviour
{
    [HideInInspector]
    public bool isBusy = false;
    public float radius = 10f;
    public float offsetDistance = 8;
    public int numberOfPoints = 8;
    [HideInInspector]
    public List<PatrolP> patrolPoints = new List<PatrolP>();

    public List<Patrol> connectedPatrolAreas = new List<Patrol>();

    void Awake()
    {
        if(patrolPoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PatrolP>())
                    patrolPoints.Add(child.GetComponent<PatrolP>());
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


        Gizmos.color = Color.yellow;
        if(connectedPatrolAreas.Count > 0)
        {
            for(int i = 0; i < connectedPatrolAreas.Count; i++)
            {
                Debug.DrawLine(transform.position, connectedPatrolAreas[i].transform.position);
            }
        }
    }

    GameObject o;
    public void GeneratePatrolPoints()
    {
        if(patrolPoints.Count > 0)
        {
            patrolPoints.Clear();
        }

        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));

        if (numberOfPoints == 0)
            Debug.LogError("Patrol points array length should not be zero.");
        else
        {
            float div = (1 / (float)numberOfPoints) * Mathf.PI * 2;
            for (int i = 0; i < numberOfPoints; i++)
            {
                Vector3 p = new Vector3(Mathf.Sin(div * (i + 1)), 0, Mathf.Cos(div * (i + 1))).normalized * offsetDistance + transform.position;
                p.y = transform.position.y;
                o = new GameObject("Patrol Point " + i.ToString());
                o.transform.position = p;
                if(Helpers.isPointOnNavMesh(o.transform.position))
                {
                    NavMeshHit hit;
                    if(NavMesh.SamplePosition(o.transform.position,out hit, 10, NavMesh.AllAreas))
                    {
                        o.transform.position = hit.position;
                    }
                }
                o.transform.parent = transform;
                PatrolP point = o.AddComponent<PatrolP>();
                patrolPoints.Add(point);
            }
        }

    }

    public bool IsPatrolAreaFree()
    {
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (!patrolPoints[i].isBusy)
                return true;
        }
        return false;
    }

    public PatrolP GrabRandomFreePatrolPoint()
    {
        List<PatrolP> nonBusyPoints = new List<PatrolP>();
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
