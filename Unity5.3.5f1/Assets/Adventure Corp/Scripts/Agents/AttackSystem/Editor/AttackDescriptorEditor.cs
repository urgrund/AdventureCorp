using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(AttackDescriptor))]
[CanEditMultipleObjects]
public class AttackDescriptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AttackDescriptor ad = target as AttackDescriptor;
       
        base.DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (GUILayout.Button("Open Animated Object in Scene", GUILayout.Height(35)))
        {
            //EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            //foreach (EditorWindow e in windows)                    
            //    Debug.Log(e.GetType());


            if (ad.clipProperties == null)
            {
                Debug.LogError("No Clip Properties, cannot continue!");
                return;
            }
            
            if (ad.clipProperties.agentAnimationProperties == null)
                return;
            if (ad.clipProperties.agentAnimationProperties.animatedGameObject == null)
                return;


            Animation o = Instantiate(ad.clipProperties.agentAnimationProperties.animatedGameObject);
            o.gameObject.name = "[DELETE ME!!!] " + ad.clipProperties.clip.name + " Animated Object";
            o.transform.position = Vector3.zero;
            o.transform.rotation = Quaternion.identity;

            o.clip = ad.clipProperties.clip;


            AttackSetupScrubber acs = o.gameObject.AddComponent<AttackSetupScrubber>();
            acs.animatedGO = o;
            acs.attackDescriptor = ad;            
            acs.agent = ad.clipProperties.agent;
            acs.attackVolumeCollection = ad.clipProperties.agent.properties.GetComponent<AttackVolumeCollection>();
            

           // Debug.Log("Vols: " + acs.attackVolumeCollection.volumes.Length + "   Bools: " + acs.attackDescriptor.volumeIndices.Count);

            Selection.activeGameObject = o.gameObject;
            SceneView.lastActiveSceneView.FrameSelected();            
        }
    }
}
