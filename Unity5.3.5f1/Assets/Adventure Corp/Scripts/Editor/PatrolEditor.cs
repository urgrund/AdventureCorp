using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Patrol))]
public class PatrolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Patrol myScript = (Patrol)target;
        if(GUILayout.Button("Generate Patrol Points"))
        {
            myScript.GeneratePatrolPoints();
        }
    }
}
