using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NPCBrain), true)]
public class NPCBrainEditor : Editor
{
	NPCBrain npc;
	NPCProfile p;	

	static float cAlpha = 0.025f;

	void OnEnable()
	{
		npc = target as NPCBrain;
		p = npc.profile;		
	}

	Vector3 OffsetPosition(float r)
	{
		return npc.transform.TransformVector(Vector3.forward * r) + npc.transform.position + (Vector3.up * 0.1f);
	}

	static void BoldLabel(string text)
	{
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField(text, EditorStyles.helpBox);
		EditorGUILayout.Space();
	}


	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if (p != null)
		{
			EditorGUILayout.LabelField("__________________________________________");
			BoldLabel("NPC Profile (" + p.name + ")");
			Editor profileEditor = Editor.CreateEditor(p);
			profileEditor.OnInspectorGUI();
		}
		else
			BoldLabel("No profile for NPC");

		
		//base.OnInspectorGUI();
	}




	void OnSceneGUI()
	{
		if (npc == null)
			return;

		if (p == null)
			return;


		if (npc.debugDraw.isActive)
		{
			Vector3 aboveHeadPos = npc.transform.position + Vector3.up * 2f;
			Handles.Label(aboveHeadPos, p.profileName + "\n" + p.statistics.rank);


			Color c;
			if (npc.debugDraw.DRAW_AWARENESS)
			{
				// Awareness				
				c = Color.magenta;
				Handles.color = c;
				Handles.Label(OffsetPosition(p.awareness.radial), "Radius");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.awareness.radial);
				c.a = cAlpha;
				Handles.color = c;
				Handles.DrawSolidArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.awareness.radial);

				c.a = 1f;
				float t = p.awareness.coneAngle / 180f;
				Vector3 f = Vector3.forward;
				Vector3 normal = Vector3.Slerp(f, -f, t);
				normal = npc.transform.TransformVector(normal);
				Vector3 position = npc.transform.position;
				Handles.color = c;
				Handles.Label(OffsetPosition(p.awareness.coneRadius), "Cone Angle & Radius");
				Handles.DrawWireArc(position, npc.transform.up, normal, p.awareness.coneAngle * 2f, p.awareness.coneRadius);
				c.a = cAlpha;
				Handles.color = c;
				Handles.DrawSolidArc(position, npc.transform.up, normal, p.awareness.coneAngle * 2f, p.awareness.coneRadius);


				// Personal space
				Handles.color = Color.white;
				Handles.Label(OffsetPosition(p.awareness.personalSpace), "Personal Space");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.awareness.personalSpace);
			}


			if (npc.debugDraw.DRAW_ATTACK)
			{
				c = Color.red;
				Handles.color = c;
				Handles.Label(OffsetPosition(p.attack.closeRangeDistance), "Close Range");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.attack.closeRangeDistance);
				Handles.Label(OffsetPosition(p.attack.farRangeDistance), "Far Range");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.attack.farRangeDistance);

				c = new Color(1f, 0.45f, 0f);
				c.a = 0.4f;
				Handles.color = c;
				Handles.Label(OffsetPosition(p.attack.stopAttackFromTargetDistance), "Stop Distance - Too fa from Target");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.attack.stopAttackFromTargetDistance);
				c.a = 0.6f;
				Handles.color = c;
				Handles.Label(OffsetPosition(p.attack.stopAttackFromStartDistance), "Stop Distance - Too far from Start");
				Handles.DrawWireArc(npc.transform.position, Vector3.up, Vector3.forward, 360f, p.attack.stopAttackFromStartDistance);

				if (Application.isPlaying)
				{
					if (npc.state == NPCBrain.State.Attack)
					{
						Handles.color = Color.red;
						Handles.DrawLine(npc.transform.position + Vector3.up, npc.target.position + Vector3.up);
						Handles.color = Color.yellow;
						Handles.DrawLine(npc.transform.position + Vector3.up, npc.attackEnteredPosition + Vector3.up);
					}
				}
			}
		}
	}
}