using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PatrolProperties
{
    public PatrolZone patrolZone;
    public PatrolPoint patrolPoint;

    public void SetProperties(PatrolZone patrolZone, PatrolPoint patrolPoint)
    {
        this.patrolZone = patrolZone;
        this.patrolPoint = patrolPoint;
    }

    public void Clear()
    {
        this.patrolPoint.isBusy = false;
        this.patrolZone = null;
        this.patrolPoint = null;
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
        PatrolZone patrolZone = null;
        PatrolPoint patrolPoint = null;

        if (!npc.patrolProperties.patrolZone) // If npc does not have a patrol zone, find the closest one to him
            patrolZone = FindNearestPatrolArea(npc.transform.position);
        else
            patrolZone = npc.patrolProperties.patrolZone; //If npc does have a patrol zone, just use that

        if (patrolZone)
        {
            if(patrolZone.connectedPatrolAreas.Count > 0) //If patrol zone is connected to other patrol zones. There is 20% chance of switching patrol zones
            {                
                if(Random.value < 0.2f)
                {
                    int index = Random.Range(0, patrolZone.connectedPatrolAreas.Count);
                    patrolZone = patrolZone.connectedPatrolAreas[index];
                }
            }

            patrolPoint = patrolZone.GrabRandomFreePatrolPoint(); //Grab a random patrol point in patrol zone
        }

        if (!patrolPoint && npc.patrolProperties.patrolPoint) //If new patrol point does not exist and NPC already has a patrol point, just stay there
            patrolPoint = npc.patrolProperties.patrolPoint;
        else if (npc.patrolProperties.patrolPoint)
            npc.patrolProperties.patrolPoint.isBusy = false; //If new patrol point exist, then set NPC patrol point busy to false

        if(patrolPoint)
            patrolPoint.isBusy = true; // Set new patrol point busy to true

        PatrolProperties p = new PatrolProperties();
        p.SetProperties(patrolZone, patrolPoint); // Set patrol properties and return to NPC

        return p;
    }

    public PatrolZone FindNearestPatrolArea(Vector3 pos)
    {
        //For all patrol zones, resort the array in order of distance from NPC
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

        //Pick the closest free patrol zone
        for(int i = 0; i < _allPatrolAreas.Count; i++)
        {
            if (_allPatrolAreas[i].IsPatrolAreaFree())
                return _allPatrolAreas[i];
        }

        return null;
    }
}
