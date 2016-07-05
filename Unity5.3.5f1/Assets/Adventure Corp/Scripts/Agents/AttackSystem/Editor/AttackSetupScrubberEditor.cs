using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AttackSetupScrubber))]
[CanEditMultipleObjects]
public class AttackSetupScrubberEditor : Editor
{     

    public override void OnInspectorGUI()
    {        
        //base.DrawDefaultInspector();

        AttackSetupScrubber c = target as AttackSetupScrubber;

        if (c.attackDescriptor == null || c.animatedGO == null)
            return;

      
        Animation a = c.animatedGO;  
        if (a != null && c.attackDescriptor != null)
        {
            // Attack Properties   
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Attack Descriptor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            c.attackDescriptor.canBeBroken = EditorGUILayout.Toggle("Can Be Broken", c.attackDescriptor.canBeBroken);
            c.attackDescriptor.minMaxRange = EditorGUILayout.Vector2Field("Suggested Use Range", c.attackDescriptor.minMaxRange);
            c.attackDescriptor.coolDownRange = EditorGUILayout.Vector2Field("Cool Down", c.attackDescriptor.coolDownRange);
            c.attackDescriptor.clipProperties = EditorGUILayout.ObjectField("Animation", c.animatedGO, typeof(AnimationClipProperties), true) as AnimationClipProperties;





            // Damage volumes        
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Volumes for this Attack", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Color guiColor = GUI.color;
            foreach (AttackVolumeDescriptor v in c.attackVolumeCollection.volumes)
            {
                bool isAdded = c.attackDescriptor.volumes.Contains(v);

                GUI.color = isAdded ? Color.yellow : Color.gray;
                if (GUILayout.Button(v.boneName))
                {
                    if (isAdded)                                            
                        c.attackDescriptor.volumes.Remove(v);                    
                    else                                            
                        c.attackDescriptor.volumes.Add(v);                    
                }
            }
            GUI.color = guiColor;





            // Damage time and animation 
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Damager Time & Animation Scrub", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            Vector2 mm = c.attackDescriptor.validDamageRange;
            EditorGUILayout.MinMaxSlider(ref mm.x, ref mm.y, 0f, 1f);
            c.scrubTime = GUILayout.HorizontalSlider(c.scrubTime, 0f, a.clip.length);
            c.attackDescriptor.validDamageRange = mm;

            a[a.clip.name].time = c.scrubTime;
            a.clip.SampleAnimation(a.gameObject, c.scrubTime);
        }
        else
        {
            EditorGUILayout.LabelField("No animation component found...");
            c.animatedGO = EditorGUILayout.ObjectField("Animation", c.animatedGO, typeof(Animation), true) as Animation;
            c.attackDescriptor = EditorGUILayout.ObjectField("Attack Descriptor", c.animatedGO, typeof(AttackDescriptor), true) as AttackDescriptor;
        }
    }   

    


    void OnSceneGUI()
    {
        AttackSetupScrubber a = target as AttackSetupScrubber;

        // Draw Attack Suggested min max range
        Handles.color = Color.yellow;
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.x);
        Handles.color = Color.red; 
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.y);
       
    }
}

