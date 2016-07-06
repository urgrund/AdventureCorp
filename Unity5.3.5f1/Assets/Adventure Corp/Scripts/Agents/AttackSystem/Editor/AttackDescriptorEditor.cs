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

        if (GUILayout.Button("Open Animated Object in Scene", GUILayout.Height(30)))
        {
            //EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            //foreach (EditorWindow e in windows)                    
            //    Debug.Log(e.GetType());


            if (ad.clipProperties == null)
            {
                Debug.LogError("No Clip Properties, cannot continue!");
                return;
            }


            if (ad.clipProperties.agent == null)
                return;
            if (ad.clipProperties.agent.animationProperties == null)
                return;
            if (ad.clipProperties.agent.animationProperties.animatedGameObject == null)
                return;


            Animation o = Instantiate(ad.clipProperties.agent.animationProperties.animatedGameObject);
            o.gameObject.name = ad.clipProperties.clip.name + " Animated Object [DELETE ME]";
            o.transform.position = Vector3.zero;
            o.transform.rotation = Quaternion.identity;

            o.clip = ad.clipProperties.clip;


            AttackSetupScrubber acs = o.gameObject.AddComponent<AttackSetupScrubber>();
            acs.animatedGO = o;
            acs.attackDescriptor = ad;
            //acs.attackDescriptor.volumes = acs.attackDescriptor.volumes.Distinct().ToList();
            acs.agent = ad.clipProperties.agent;
            acs.attackVolumeCollection = ad.clipProperties.agent.properties.GetComponent<AttackVolumeCollection>();

            // Check bools
            // fist if never created            
            //if (acs.attackDescriptor.bools == null)
            //{
            //    acs.attackDescriptor.bools = new List<bool>();
            //    acs.attackDescriptor.bools.AddRange(Enumerable.Repeat(false, acs.attackVolumeCollection.volumes.Length));
            //}
            //else
            //{
            //    acs.attackDescriptor.bools = Helpers.ListResize(acs.attackDescriptor.bools, acs.attackVolumeCollection.volumes.Length, false);                
            //}

            Debug.Log("Vols: " + acs.attackVolumeCollection.volumes.Length + "   Bools: " + acs.attackDescriptor.volumeIndices.Count);

            Selection.activeGameObject = o.gameObject;
            SceneView.lastActiveSceneView.FrameSelected();            
        }
    }
}
