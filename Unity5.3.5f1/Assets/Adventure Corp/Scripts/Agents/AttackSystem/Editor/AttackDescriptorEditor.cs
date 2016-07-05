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

        if (GUILayout.Button("Open Animated Object in Scene"))
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


            {

                Animation o = Instantiate(ad.clipProperties.agent.animationProperties.animatedGameObject);
                o.gameObject.name = ad.clipProperties.clip.name + " Animated Object [DELETE ME]";
                o.transform.position = Vector3.zero;
                o.transform.rotation = Quaternion.identity;

                o.clip = ad.clipProperties.clip;


                AttackSetupScrubber acs = o.gameObject.AddComponent<AttackSetupScrubber>();
                acs.animatedGO = o;
                acs.attackDescriptor = ad;
                acs.attackDescriptor.volumes = acs.attackDescriptor.volumes.Distinct().ToList();
                acs.agent = ad.clipProperties.agent;
                acs.attackVolumeCollection = ad.clipProperties.agent.properties.GetComponent<AttackVolumeCollection>();

                Selection.activeGameObject = o.gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
            }
            //EditorApplication.ExecuteMenuItem("Window/Animation");
        }
    }
}
