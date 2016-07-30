using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PatrolProperties
{
    public PatrolZone patrolArea;
    public PatrolPoint patrolPoint;

    public void SetProperties(PatrolZone patrolArea, PatrolPoint patrolPoint)
    {
        this.patrolArea = patrolArea;
        this.patrolPoint = patrolPoint;
    }
}

public class PatrolManager : MonoBehaviour
{
    private static PatrolManager _instance = null; 
    public static PatrolManager instance { get { return _instance; } }

    private List<PatrolZone> _allPatrolAreas= new List<PatrolZone>(); // All objects with a health script are found here
    public static List<PatrolZone> allPatrolAreas { get { return _instance._allPatrolAreas; } }

    void Awake()
    {
        _instance = this;
    }

    public static void Register(PatrolZone p)
    {
        if (_instance == null)
        {
            Debug.LogWarning("No Patrol Manager In Scene");
            return;
        }

        allPatrolAreas.Add(p);
    }

    public static void DeRegister(PatrolZone p)
    {
        if (_instance == null)
        {
            Debug.LogWarning("No Patrol Manager In Scene");
            return;
        }

        allPatrolAreas.Remove(p);
    }

    public PatrolProperties GrabPatrolProperties(NPCBrain npc)
    {
        PatrolZone patrolArea = null;
        PatrolPoint patrolPoint = null;

        if (!npc.patrolProperties.patrolArea)
            patrolArea = FindNearestPatrolArea(npc.transform.position);
        else
            patrolArea = npc.patrolProperties.patrolArea;

        if (patrolArea)
        {
            if(patrolArea.connectedPatrolAreas.Count > 0)
            {
                float rand = Random.Range(0.0f, 100.0f);
                if(rand < 20)
                {
                    int index = Random.Range(0, patrolArea.connectedPatrolAreas.Count);
                    patrolArea = patrolArea.connectedPatrolAreas[index];
                }
            }

            patrolPoint = patrolArea.GrabRandomFreePatrolPoint();
        }

        if (!patrolPoint && npc.patrolProperties.patrolPoint)
            patrolPoint = npc.patrolProperties.patrolPoint;
        else if (npc.patrolProperties.patrolPoint)
            npc.patrolProperties.patrolPoint.isBusy = false;

        if(patrolPoint)
            patrolPoint.isBusy = true;

        PatrolProperties p = new PatrolProperties();
        p.SetProperties(patrolArea, patrolPoint);

        return p;
    }

    public PatrolZone FindNearestPatrolArea(Vector3 pos)
    {
        for(int i = _allPatrolAreas.Count - 1; i >= 0; i--)
        {
            for(int j = 1; j <= i; j++)
            {
                float d1 = Vector3.Distance(_allPatrolAreas[j - 1].transform.position, pos);
                float d2 = Vector3.Distance(_allPatrolAreas[j].transform.position, pos);
                if(d1 > d2)
                {
                    PatrolZone temp = _allPatrolAreas[j - 1];
                    _allPatrolAreas[j - 1] = _allPatrolAreas[j];
                    _allPatrolAreas[j] = temp;
                }
            }
        }

        for(int i = 0; i < _allPatrolAreas.Count; i++)
        {
            if (_allPatrolAreas[i].IsPatrolAreaFree())
                return _allPatrolAreas[i];
        }

        return null;
    }
}
