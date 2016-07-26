using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PatrolZone))]
[CanEditMultipleObjects]
public class PatrolZoneEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		PatrolZone myScript = (PatrolZone)target;
		if (GUILayout.Button("Generate Patrol Points In Circle"))
		{
			myScript.GeneratePatrolPointsInCircle();
		}
        if (GUILayout.Button("Generate Patrol Points Randomly"))
        {
            myScript.GeneratePatrolPointsRandomly();
        }
        if (GUILayout.Button("Sample Patrol Points"))
        {
            myScript.SamplePatrolPointsOnNavMesh();
        }
    }

	void OnSceneGUI()
	{
		PatrolZone p = target as PatrolZone;
		Color c = new Color(255/255, 165/255, 0);		
		Handles.color = c;
		foreach (PatrolZone pz in p.connectedPatrolAreas)
			Handles.DrawLine(pz.transform.position, p.transform.position);
	}


	[MenuItem("CONTEXT/PatrolZone/Link Selected Patrol Zones")]
	private static void LinkPatrolZones(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;
		foreach (Object o in Selection.objects)
		{
			PatrolZone p = (o as GameObject).GetComponent<PatrolZone>();
			if (p != thisSelectedPZ)
				thisSelectedPZ.connectedPatrolAreas.Add(p);
		}
	}

	[MenuItem("CONTEXT/PatrolZone/UnLink Selected Patrol Zones")]
	private static void UnLinkPatrolZones(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;
		foreach (Object o in Selection.objects)
		{
			PatrolZone p = (o as GameObject).GetComponent<PatrolZone>();
			p.connectedPatrolAreas.Remove(thisSelectedPZ);
			thisSelectedPZ.connectedPatrolAreas.Remove(p);
		}
	}

	[MenuItem("CONTEXT/PatrolZone/Unlink All Connected Zones")]
	private static void UnLinkAll(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;
		foreach (PatrolZone p in thisSelectedPZ.connectedPatrolAreas)
			p.connectedPatrolAreas.Remove(thisSelectedPZ);
		thisSelectedPZ.connectedPatrolAreas.Clear();
	}

	


}
