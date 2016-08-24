using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]	
public class PatrolManager
{
	private static PatrolManager _instance = null;
	public static PatrolManager instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.Log("! Creating Instance of " + typeof(PatrolManager));
				_instance = new PatrolManager();
				UnityEngine.SceneManagement.SceneManager.sceneUnloaded += _instance.OnSceneWasUnLoaded;	
			}
			return _instance;
		}
	}


	void OnSceneWasUnLoaded(UnityEngine.SceneManagement.Scene scene)
	{		
		_instance = null;
	}

	public List<PatrolZone> _patrolZonesInScene = new List<PatrolZone>(); // All objects with a health script are found here
	public static List<PatrolZone> patrolZonesInScene { get { return _instance._patrolZonesInScene; } }
	
	public static void Register(PatrolZone p)
	{
		if (!instance._patrolZonesInScene.Contains(p))
			instance._patrolZonesInScene.Add(p);
		else
			Debug.LogWarning("Trying to add a PatrolZone more than once?   "  + p);
	}

	public static void DeRegister(PatrolZone p)
	{
		instance._patrolZonesInScene.Remove(p);
	}

	public static PatrolZone GetNearestPatrolZone(Transform transform) { return GetNearestPatrolZone(transform.position); }
	public static PatrolZone GetNearestPatrolZone(Vector3 position)
	{
		List<Transform> transforms = new List<Transform>();
		foreach (PatrolZone pz in instance._patrolZonesInScene)
			transforms.Add(pz.transform);
		if (transforms == null || transforms.Count == 0)
			return null;
		return Helpers.FindClosestTransform(transforms.ToArray(), position).GetComponent<PatrolZone>(); 
	}	
}












/*public struct PatrolProperties
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
}*/








// public PatrolProperties GrabPatrolProperties(NPCBrain npc)
// {
//	return new PatrolProperties();
//    PatrolZone patrolZone = null;
//    PatrolPoint patrolPoint = null;

//    if (!npc.patrolProperties.patrolZone) // If npc does not have a patrol zone, find the closest one to him
//        patrolZone = FindNearestPatrolArea(npc.transform.position);
//    else
//        patrolZone = npc.patrolProperties.patrolZone; //If npc does have a patrol zone, just use that

//    if (patrolZone)
//    {
//        if(patrolZone.connectedPatrolAreas.Count > 0) 
//        {                
//            //if(Random.value < 0.9f)
//            {
//	//int index = Random.Range(0, patrolZone.connectedPatrolAreas.Count);
//	//patrolZone = patrolZone.connectedPatrolAreas[index];			
//	patrolZone = patrolZone.connectedPatrolAreas[0];
//}
//        }

//        patrolPoint = patrolZone.GrabRandomFreePatrolPoint(); //Grab a random patrol point in patrol zone
//    }

//    if (!patrolPoint && npc.patrolProperties.patrolPoint) //If new patrol point does not exist and NPC already has a patrol point, just stay there
//        patrolPoint = npc.patrolProperties.patrolPoint;
//    else if (npc.patrolProperties.patrolPoint)
//        npc.patrolProperties.patrolPoint.isBusy = false; //If new patrol point exist, then set NPC patrol point busy to false

//    if(patrolPoint)
//        patrolPoint.isBusy = true; // Set new patrol point busy to true

//    PatrolProperties p = new PatrolProperties();
//    p.SetProperties(patrolZone, patrolPoint); // Set patrol properties and return to NPC

//    return p;
// }

// public PatrolZone FindNearestPatrolArea(Vector3 pos)
// {
//For all patrol zones, resort the array in order of distance from NPC
//for(int i = _allPatrolAreas.Count - 1; i >= 0; i--)
//{
//    for(int j = 1; j <= i; j++)
//    {
//        float d1 = Vector3.Distance(_allPatrolAreas[j - 1].transform.position, pos);
//        float d2 = Vector3.Distance(_allPatrolAreas[j].transform.position, pos);
//        if(d1 > d2)
//        {
//            PatrolZone temp = _allPatrolAreas[j - 1];
//            _allPatrolAreas[j - 1] = _allPatrolAreas[j];
//            _allPatrolAreas[j] = temp;
//        }
//    }
//}

//Pick the closest free patrol zone
//for(int i = 0; i < _allPatrolAreas.Count; i++)
//{
//    if (_allPatrolAreas[i].IsPatrolAreaFree())
//        return _allPatrolAreas[i];
//}

//    return null;
//  }