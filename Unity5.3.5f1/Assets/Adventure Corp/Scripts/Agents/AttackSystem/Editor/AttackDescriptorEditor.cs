using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(AttackDescriptor))]
[CanEditMultipleObjects]
public class AttackDescriptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AttackDescriptor ad = target as AttackDescriptor;
        //base.OnInspectorGUI();

        ad.canBeBroken = EditorGUILayout.Toggle("Can Be Broken", ad.canBeBroken);
        ad.minMaxRange = EditorGUILayout.Vector2Field("Suggested Use Range", ad.minMaxRange);
        ad.coolDownRange = EditorGUILayout.Vector2Field("Cool Down", ad.coolDownRange);

        EditorGUILayout.Space();
        ad.damage = EditorGUILayout.ObjectField("Damage", ad.damage, typeof(Damage), false) as Damage;
        ad.validDamageRange = EditorGUILayout.Vector2Field("Damager Time", ad.validDamageRange);
        EditorGUILayout.MinMaxSlider(new GUIContent("Damager Time"), ref ad.validDamageRange.x, ref ad.validDamageRange.y, 0f, 1f);
        EditorGUILayout.Space();

        ad.clipProperties= EditorGUILayout.ObjectField("Animation Clip Properites", ad.clipProperties, typeof(AnimationClipProperties), false) as AnimationClipProperties;

        if (GUILayout.Button("Open Animated Object in Scene"))
        {
            //EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            //foreach (EditorWindow e in windows)                    
            //    Debug.Log(e.GetType());

            Animation o = Instantiate(ad.clipProperties.animatedGameObject);
            o.gameObject.name = ad.clipProperties.clip.name + " Animated Object [DELETE ME]";
            o.transform.position = Vector3.zero;
            o.transform.rotation = Quaternion.identity;

            o.clip = ad.clipProperties.clip;

            AnimationClipScrubber acs = o.gameObject.AddComponent<AnimationClipScrubber>();
            acs.animatedGO = o;
            acs.attackDescriptor = ad;

            Selection.activeGameObject = o.gameObject;
            SceneView.lastActiveSceneView.FrameSelected();

            //EditorApplication.ExecuteMenuItem("Window/Animation");
        }
    }
}
