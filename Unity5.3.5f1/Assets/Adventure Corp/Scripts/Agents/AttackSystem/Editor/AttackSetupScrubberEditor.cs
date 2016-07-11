using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttackSetupScrubber))]
[CanEditMultipleObjects]
public class AttackSetupScrubberEditor : Editor
{

    SerializedObject attDesc;

    SerializedProperty canBeBroken;
    SerializedProperty lockController;
    SerializedProperty useCurves;
    SerializedProperty suggestedUseRange;
    SerializedProperty suggestedUseAngle;
    SerializedProperty validDamageRange;
    SerializedProperty volumeIndices;
    SerializedProperty clipProperties;
    SerializedProperty damage;

    bool[] tempVolumeIndices; 

    
    void OnEnable()
    {
        if (target == null)
            return;

        // Setup serialized properties
        AttackSetupScrubber t = target as AttackSetupScrubber;
        attDesc = new SerializedObject(t.attackDescriptor);

        canBeBroken = attDesc.FindProperty("canBeBroken");
        lockController = attDesc.FindProperty("lockController");
        useCurves = attDesc.FindProperty("useCurves");
        suggestedUseRange = attDesc.FindProperty("suggestedUseRange");
        suggestedUseAngle = attDesc.FindProperty("suggestedUseAngle");
        validDamageRange = attDesc.FindProperty("validDamageRange");
        volumeIndices = attDesc.FindProperty("volumeIndices");
        clipProperties = attDesc.FindProperty("clipProperties");
        damage = attDesc.FindProperty("damage");


        // Setup volumes
        // Create a temporary array of bools for the indices that 
        // is the size of all current available volumes for the attack 
        tempVolumeIndices = new bool[t.attackVolumeCollection.volumes.Length];
        t.volumeIndices = new bool[t.attackVolumeCollection.volumes.Length];
        volumeIndices.arraySize = t.attackVolumeCollection.volumes.Length;
        attDesc.ApplyModifiedProperties();

        for (int i = 0; i < tempVolumeIndices.Length; i++)
        {
            if (i > volumeIndices.arraySize - 1)
            {
                tempVolumeIndices[i] = false;                
            }
            else
                tempVolumeIndices[i] = volumeIndices.GetArrayElementAtIndex(i).boolValue;
        }
    }



    AttackDescriptor.Angle range = AttackDescriptor.Angle.Narrow;

    AnimationCurve curveX = AnimationCurve.Linear(0, 0, 1, 0);
    AnimationCurve curveY = AnimationCurve.Linear(0, 0, 1, 0);
    AnimationCurve curveZ = AnimationCurve.Linear(0, 0, 1, 0);
    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();

        attDesc.Update();

        AttackSetupScrubber c = target as AttackSetupScrubber;
        Animation a = c.animatedGO;

        BoldLabel("Attack Properties");
        EditorGUILayout.PropertyField(canBeBroken);
        EditorGUILayout.PropertyField(lockController);
        EditorGUILayout.PropertyField(useCurves);
        EditorGUILayout.PropertyField(suggestedUseRange);
        //EditorGUILayout.PropertyField(suggestedUseAngle);   
        EditorGUILayout.PropertyField(damage, true);
        EditorGUILayout.PropertyField(clipProperties, true);
        //EditorGUILayout.PropertyField(volumeIndices, true);

        range = (AttackDescriptor.Angle)EditorGUILayout.EnumPopup("Angle Test", range);
        suggestedUseAngle.floatValue = (float)((int)range);

        BoldLabel("Volumes used for this attack");
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            tempVolumeIndices[i] = GUILayout.Toggle(tempVolumeIndices[i], c.attackVolumeCollection.volumes[i].boneName);
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            volumeIndices.GetArrayElementAtIndex(i).boolValue = tempVolumeIndices[i];
        c.volumeIndices = tempVolumeIndices;


        BoldLabel("Damage Range and Movement Curves" + "   (scrub time : " + (c.scrubTime/a.clip.length)+ " )");
        Rect constraints = new Rect(0, 0, 1, 10);
        curveX = EditorGUILayout.CurveField(curveX, Color.red, constraints, null);
        curveY = EditorGUILayout.CurveField(curveY, Color.green, constraints, null);
        curveZ = EditorGUILayout.CurveField(curveZ, Color.blue, constraints, null);
        Vector2 mm = c.attackDescriptor.validDamageRange;
        EditorGUILayout.MinMaxSlider(ref mm.x, ref mm.y, 0f, 1f);

        GUI.color = Color.grey;
        c.scrubTime = GUILayout.HorizontalSlider(c.scrubTime, 0f, a.clip.length);
        validDamageRange.vector2Value = mm;
        a[a.clip.name].time = c.scrubTime;
        a.clip.SampleAnimation(a.gameObject, c.scrubTime);

        if (useCurves.boolValue)
            UpdateModelPositionWithCurves(c.scrubTime / a.clip.length);

        // Apply changes
        attDesc.ApplyModifiedProperties();
    }

    Vector3 lastCurvePosition = Vector3.zero;
    void UpdateModelPositionWithCurves(float t)
    {        
        lastCurvePosition.x = curveX.Evaluate(t);
        lastCurvePosition.y = curveY.Evaluate(t);
        lastCurvePosition.z = curveZ.Evaluate(t);
        (target as AttackSetupScrubber).transform.position = lastCurvePosition;
    }

    static void BoldLabel(string text)
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        EditorGUILayout.Space();
    }



    public void OnDestroy()
    {
        AssetDatabase.SaveAssets();
    }



    void OnSceneGUI()
    {
        attDesc.Update();
        AttackSetupScrubber a = target as AttackSetupScrubber;

        Handles.color = Color.white;
        Handles.Label(a.transform.position + Vector3.up * 4f, a.attackDescriptor.name);
        DrawArcs(a);
       
        if (a.volumePositions != null)
        {
            for (int i = 0; i < a.attackVolumeCollection.volumes.Length; i++)
            {
                Handles.color = tempVolumeIndices[i] ? AttackSetupScrubber.c_inUse : AttackSetupScrubber.c_inActive;

                float t = a.animatedGO[a.animatedGO.clip.name].normalizedTime;
                if (t < a.attackDescriptor.validDamageRange.y && t > a.attackDescriptor.validDamageRange.x)
                    Handles.color = AttackSetupScrubber.c_inRange;
                
                Handles.Label(a.volumePositions[i], a.attackVolumeCollection.volumes[i].boneName);
            }
        }

        attDesc.ApplyModifiedProperties();
    }

    void DrawArcs(AttackSetupScrubber a)
    {
        // Draw Attack Suggested min max range        
        float t = suggestedUseAngle.floatValue / 360f;
        Vector3 normal = Vector3.Slerp(Vector3.forward, -Vector3.forward, t);
        Vector3 position = a.transform.position - lastCurvePosition;


        Handles.color = Color.red;
        Handles.DrawWireArc(position, Vector3.up, normal, suggestedUseAngle.floatValue, suggestedUseRange.vector2Value.x);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(position, Vector3.up, normal, suggestedUseAngle.floatValue,  suggestedUseRange.vector2Value.y);        

        Handles.color = Color.yellow * new Color(1, 1, 1, 0.02f);
        Handles.DrawSolidArc(position, Vector3.up, normal, suggestedUseAngle.floatValue, suggestedUseRange.vector2Value.y);

        Handles.color = Color.white;
        Vector2 sur = suggestedUseRange.vector2Value;
        sur.x = Handles.ScaleValueHandle(sur.x,
                        position + a.transform.forward * sur.x,
                        a.transform.rotation, 1, Handles.ConeCap, 1);
        sur.y = Handles.ScaleValueHandle(sur.y,
                        position + a.transform.forward * sur.y,
                        a.transform.rotation, 1, Handles.ConeCap, 1);
        suggestedUseRange.vector2Value = sur;
    }
}

