using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttackSetupScrubber))]
[CanEditMultipleObjects]
public class AttackSetupScrubberEditor : Editor
{

    SerializedObject attDesc;

    SerializedProperty canBeBroken;
    SerializedProperty minMaxRange;
    SerializedProperty coolDownRange;
    SerializedProperty validDamageRange;
    SerializedProperty volumeIndices;
    SerializedProperty clipProperties;
    SerializedProperty damage;
    bool[] tempVolumeIndices; 

    //bool canBeBroken;

    void OnEnable()
    {
        // Setup serialized properties
        AttackSetupScrubber t = target as AttackSetupScrubber;
        attDesc = new SerializedObject(t.attackDescriptor);

        canBeBroken = attDesc.FindProperty("canBeBroken");
        minMaxRange = attDesc.FindProperty("minMaxRange");
        coolDownRange = attDesc.FindProperty("coolDownRange");
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

    public override void OnInspectorGUI()
    {
        //base.DrawDefaultInspector();

        attDesc.Update();

        AttackSetupScrubber c = target as AttackSetupScrubber;
        Animation a = c.animatedGO;

        BoldLabel("Attack Properties");
        EditorGUILayout.PropertyField(canBeBroken);
        EditorGUILayout.PropertyField(minMaxRange, new GUIContent("Suggested Use Range"));
        EditorGUILayout.PropertyField(coolDownRange);        
        EditorGUILayout.PropertyField(damage, true);
        EditorGUILayout.PropertyField(clipProperties, true);
        //EditorGUILayout.PropertyField(volumeIndices, true);


        BoldLabel("Volumes used for this attack");
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            tempVolumeIndices[i] = GUILayout.Toggle(tempVolumeIndices[i], c.attackVolumeCollection.volumes[i].boneName);
        for (int i = 0; i < tempVolumeIndices.Length; i++)
            volumeIndices.GetArrayElementAtIndex(i).boolValue = tempVolumeIndices[i];
        c.volumeIndices = tempVolumeIndices;

                
        BoldLabel("Damage Range & Animation Scrub");
        Vector2 mm = c.attackDescriptor.validDamageRange;
        EditorGUILayout.MinMaxSlider(ref mm.x, ref mm.y, 0f, 1f);
        c.scrubTime = GUILayout.HorizontalSlider(c.scrubTime, 0f, a.clip.length);
        validDamageRange.vector2Value = mm;
        a[a.clip.name].time = c.scrubTime;
        a.clip.SampleAnimation(a.gameObject, c.scrubTime);

        
        // Apply changes
        attDesc.ApplyModifiedProperties();        
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
        AttackSetupScrubber a = target as AttackSetupScrubber;

        // Draw Attack Suggested min max range
        Handles.color = Color.yellow;
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.x);
        Handles.color = Color.red;
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.y);

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
    }
}

