using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PatrolZone))]
[CanEditMultipleObjects]
public class PatrolZoneEditor : Editor
{
	//public override void OnInspectorGUI()
	//{
	//	DrawDefaultInspector();		
 //   }	

	[MenuItem("CONTEXT/PatrolZone/Link Selected Patrol Zones")]
	private static void LinkPatrolZones(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;
		foreach (Object o in Selection.objects)
		{
			PatrolZone p = (o as GameObject).GetComponent<PatrolZone>();
			if (p != thisSelectedPZ)
				thisSelectedPZ.connectedPatrolZones.Add(p);
		}
	}

	[MenuItem("CONTEXT/PatrolZone/UnLink Selected Patrol Zones")]
	private static void UnLinkPatrolZones(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;
		foreach (Object o in Selection.objects)
		{
			PatrolZone p = (o as GameObject).GetComponent<PatrolZone>();
			p.RemoveConnectedPatrolZone(thisSelectedPZ);
			thisSelectedPZ.RemoveConnectedPatrolZone(p);
		}
	}

	[MenuItem("CONTEXT/PatrolZone/Unlink All Connected Zones")]
	private static void UnLinkAll(MenuCommand menuCommand)
	{
		PatrolZone thisSelectedPZ = menuCommand.context as PatrolZone;		
		thisSelectedPZ.ClearAllConnectedPatrolZones();
	}
}
