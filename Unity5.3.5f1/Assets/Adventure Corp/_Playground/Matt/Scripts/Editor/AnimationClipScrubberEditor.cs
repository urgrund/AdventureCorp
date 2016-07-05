using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AnimationClipScrubber))]
[CanEditMultipleObjects]
public class AnimationClipScrubberEditor : Editor
{    

    public override void OnInspectorGUI()
    {
        AnimationClipScrubber c = target as AnimationClipScrubber;

        if (c.attackDescriptor == null || c.animatedGO == null)
            return;
        Vector2 mm = c.attackDescriptor.validDamageRange;


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Damager Time & Animation Scrub", EditorStyles.largeLabel);

        Animation a = c.animatedGO;  //c.transform.GetComponent<Animation>();
        if (a != null && c.attackDescriptor != null)
        {
            c.attackDescriptor.canBeBroken = EditorGUILayout.Toggle("Can Be Broken", c.attackDescriptor.canBeBroken);
            c.attackDescriptor.minMaxRange = EditorGUILayout.Vector2Field("Suggested Use Range", c.attackDescriptor.minMaxRange);
            c.attackDescriptor.coolDownRange = EditorGUILayout.Vector2Field("Cool Down", c.attackDescriptor.coolDownRange);


            //EditorGUILayout.LabelField("Normalized Time : " + a[a.clip.name].normalizedTime.ToString());
            //EditorGUILayout.LabelField("Animation Time  : " + a[a.clip.name].time.ToString());

            EditorGUILayout.MinMaxSlider(ref mm.x, ref mm.y, 0f, 1f);
            c.scrubTime = GUILayout.HorizontalSlider(c.scrubTime, 0f, a.clip.length);
            c.attackDescriptor.validDamageRange = mm;

            a[a.clip.name].time = c.scrubTime;
            a.clip.SampleAnimation(a.gameObject, c.scrubTime);

            //float t = ((float)EditorApplication.timeSinceStartup % a.clip.length);
            //a.clip.SampleAnimation(a.gameObject, t);
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
        AnimationClipScrubber a = target as AnimationClipScrubber;

        Handles.color = Color.yellow;
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.x);
        Handles.color = Color.red; 
        Handles.DrawWireArc(a.transform.position, Vector3.up, Vector3.forward, 360f, a.attackDescriptor.minMaxRange.y);

        float t = a.animatedGO[a.animatedGO.clip.name].normalizedTime;

        foreach (SkinnedMeshRenderer r in (target as AnimationClipScrubber).GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (t < a.attackDescriptor.validDamageRange.y && t > a.attackDescriptor.validDamageRange.x)
                r.sharedMaterial.color = Color.red;
            else
                r.sharedMaterial.color = Color.white;
        }
    }
}

