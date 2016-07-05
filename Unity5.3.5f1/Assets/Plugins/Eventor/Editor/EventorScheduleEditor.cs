using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(EventorSchedule))]
public class EventorScheduleEditor : Editor
{
	public override void OnInspectorGUI ()
	{
		if(GUILayout.Button("Open Editor", GUILayout.Height(40)))
		{
			EventorScheduleEditorWindow window = (EventorScheduleEditorWindow) EditorWindow.GetWindow(typeof(EventorScheduleEditorWindow));
			window.Init(target as EventorSchedule);
		}

		if(GUILayout.Button("Run Schedule", GUILayout.Height(30)))
		{
			(target as EventorSchedule).Run();
		}
		
		base.OnInspectorGUI ();
	}

	void OnSceneGUI()
	{
		foreach(EventorJob j in (target as EventorSchedule).GetComponentsInChildren<EventorJob>())
		{
			Color a = new Color(1,1,1,0.5f);

			Handles.color = j.isActive ? Color.green * a : Color.grey * a;
			Handles.DrawLine((target as EventorSchedule).transform.position, j.transform.position);
			Handles.CubeCap(0, j.transform.position, Quaternion.identity, 0.25f);
		}

		Handles.color = Color.yellow;
		Handles.CubeCap(0, (target as EventorSchedule).transform.position, Quaternion.identity, 0.25f);
	}
}
