using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PatrolProperties
{
    public Patrol patrolArea;
    public PatrolP patrolPoint;

    public void SetProperties(Patrol patrolArea, PatrolP patrolPoint)
    {
        this.patrolArea = patrolArea;
        this.patrolPoint = patrolPoint;
    }
}

public class PatrolManager : MonoBehaviour
{
    private static PatrolManager _instance = null; // LevelManager is a singleton
    public static PatrolManager instance { get { return _instance; } }

    private List<Patrol> _allPatrolAreas= new List<Patrol>(); // All objects with a health script are found here
    public static List<Patrol> allPatrolAreas { get { return _instance._allPatrolAreas; } }

    void Awake()
    {
        _instance = this;
    }

    public static void Register(Patrol p)
    {
        if (_instance == null)
        {
            Debug.LogWarning("No Patrol Manager In Scene");
            return;
        }

        allPatrolAreas.Add(p);
    }

    public static void DeRegister(Patrol p)
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
        Patrol patrolArea = null;
        PatrolP patrolPoint = null;

        if (!npc.patrolProperties.patrolArea)
            patrolArea = FindNearestPatrolArea(npc.transform.position);
        else
            patrolArea = npc.patrolProperties.patrolArea;

        if (patrolArea)
            patrolPoint = patrolArea.GrabRandomFreePatrolPoint();

        if (npc.patrolProperties.patrolPoint)
            npc.patrolProperties.patrolPoint.isBusy = false;

        if(patrolPoint)
            patrolPoint.isBusy = true;

        PatrolProperties p = new PatrolProperties();
        p.SetProperties(patrolArea, patrolPoint);

        return p;
    }

    public Patrol FindNearestPatrolArea(Vector3 pos)
    {
        for(int i = _allPatrolAreas.Count - 1; i >= 0; i--)
        {
            for(int j = 1; j <= i; j++)
            {
                float d1 = Vector3.Distance(_allPatrolAreas[j - 1].transform.position, pos);
                float d2 = Vector3.Distance(_allPatrolAreas[j].transform.position, pos);
                if(d1 > d2)
                {
                    Patrol temp = _allPatrolAreas[j - 1];
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
