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

    SerializedProperty canAttackBeParried;
    SerializedProperty controllerLock;
	SerializedProperty controllerGravity;
	SerializedProperty suggestedUseRange;
    SerializedProperty suggestedUseAngle;
    SerializedProperty validDamageRange;
	SerializedProperty turnToTargetRatio;
    SerializedProperty volumeIndices;
    SerializedProperty clipProperties;
    SerializedProperty eventor;
    SerializedProperty damage;

    SerializedProperty curveX;
    SerializedProperty curveY;
    SerializedProperty curveZ;



    AttackDescriptor.Angle range = AttackDescriptor.Angle.Narrow;

    



    bool[] tempVolumeIndices; 

    
    void OnEnable()
    {
        if (target == null)
            return;

        // Setup serialized properties
        AttackSetupScrubber t = target as AttackSetupScrubber;
        attDesc = new SerializedObject(t.attackDescriptor);

		canAttackBeParried = attDesc.FindProperty("canAttackBeParried");
        controllerLock = attDesc.FindProperty("controllerLock");
		controllerGravity = attDesc.FindProperty("controllerGravity");
		suggestedUseRange = attDesc.FindProperty("suggestedUseRange");
        suggestedUseAngle = attDesc.FindProperty("suggestedUseAngle");
		turnToTargetRatio = attDesc.FindProperty("turnToTargetRatio");
		validDamageRange = attDesc.FindProperty("validDamageRange");
        volumeIndices = attDesc.FindProperty("volumeIndices");
        clipProperties = attDesc.FindProperty("clipProperties");
        eventor = attDesc.FindProperty("eventor");
        damage = attDesc.FindProperty("damage");

        curveX = attDesc.FindProperty("curveX");
        curveY = attDesc.FindProperty("curveY");
        curveZ = attDesc.FindProperty("curveZ");


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



    


    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();

        attDesc.Update();

        AttackSetupScrubber c = target as AttackSetupScrubber;
        Animation a = c.animatedGO;

        BoldLabel("Attack Properties");
        EditorGUILayout.PropertyField(canAttackBeParried);
        EditorGUILayout.PropertyField(controllerLock);
		EditorGUILayout.PropertyField(controllerGravity);
		EditorGUILayout.PropertyField(suggestedUseRange);
		EditorGUILayout.PropertyField(suggestedUseAngle);
        EditorGUILayout.PropertyField(damage, true);
        EditorGUILayout.PropertyField(clipProperties, true);
        EditorGUILayout.PropertyField(eventor, true);        


        //range = (AttackDescriptor.Angle)EditorGUILayout.EnumPopup("Use Range", range);
        //suggestedUseAngle.floatValue = (float)((int)range);

        BoldLabel("Volumes used for this attack");
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            tempVolumeIndices[i] = GUILayout.Toggle(tempVolumeIndices[i], c.attackVolumeCollection.volumes[i].boneName);
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            volumeIndices.GetArrayElementAtIndex(i).boolValue = tempVolumeIndices[i];
        c.volumeIndices = tempVolumeIndices;


        // Clip time is it's length scaled by play speed
        float clipTime = (a.clip.length * (1/c.attackDescriptor.clipProperties.playSpeed)) * c.scrubTime;

        BoldLabel("Damage Range and Movement Curves");
        EditorGUILayout.LabelField("Clip Length/Time - (" + (c.scrubTime/a.clip.length)+ "/" + clipTime + ")", EditorStyles.miniLabel);
		float constraintLimit = 5f;
        Rect constraints = new Rect(0, -constraintLimit, 1, constraintLimit*2f);
        curveX.animationCurveValue = EditorGUILayout.CurveField(curveX.animationCurveValue, Color.red, constraints, null);
        curveY.animationCurveValue = EditorGUILayout.CurveField(curveY.animationCurveValue, Color.green, constraints, null);
        curveZ.animationCurveValue = EditorGUILayout.CurveField(curveZ.animationCurveValue, Color.cyan, constraints, null);

		EditorGUILayout.LabelField("Valid Damage Range", EditorStyles.miniLabel);
		Vector2 mm = c.attackDescriptor.validDamageRange;
        EditorGUILayout.MinMaxSlider(ref mm.x, ref mm.y, 0f, 1f);

		EditorGUILayout.LabelField("Turn to Target Ratio", EditorStyles.miniLabel);
		turnToTargetRatio.floatValue = GUILayout.HorizontalSlider(turnToTargetRatio.floatValue, 0, 1);


		EditorGUILayout.LabelField("Animation Clip Scrubbing", EditorStyles.miniLabel);
		GUI.color = Color.grey;
        c.scrubTime = GUILayout.HorizontalSlider(c.scrubTime, 0f, a.clip.length);
        validDamageRange.vector2Value = mm;
        a[a.clip.name].time = c.scrubTime;
        a.clip.SampleAnimation(a.gameObject, c.scrubTime);

        if ((AttackDescriptor.Lock)controllerLock.enumValueIndex == AttackDescriptor.Lock.Curves)
            UpdateModelPositionWithCurves(c.scrubTime / a.clip.length);

        // Apply changes
        attDesc.ApplyModifiedProperties();
    }

    Vector3 curvePosition = Vector3.zero;
    void UpdateModelPositionWithCurves(float t)
    {
        curvePosition.x = curveX.animationCurveValue.Evaluate(t);
        curvePosition.y = curveY.animationCurveValue.Evaluate(t);
        curvePosition.z = curveZ.animationCurveValue.Evaluate(t);
        (target as AttackSetupScrubber).transform.position = curvePosition;
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
		float angle = AttackDescriptor.AngleFromIndex(suggestedUseAngle.enumValueIndex);

		// Draw Attack Suggested min max range        
		float t = angle / 360f;
        Vector3 normal = Vector3.Slerp(Vector3.forward, -Vector3.forward, t);
        Vector3 position = a.transform.position - curvePosition;

        Handles.color = Color.red;
        Handles.DrawWireArc(position, Vector3.up, normal, angle, suggestedUseRange.vector2Value.x);
        Handles.color = Color.yellow;
        Handles.DrawWireArc(position, Vector3.up, normal, angle,  suggestedUseRange.vector2Value.y);        

        Handles.color = Color.yellow * new Color(1, 1, 1, 0.02f);
        Handles.DrawSolidArc(position, Vector3.up, normal, angle, suggestedUseRange.vector2Value.y);

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

