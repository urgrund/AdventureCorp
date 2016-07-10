using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class CreateAnimationClipFromSelection
{
    T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }



    // -----------------------------------------------
    // Create anim properties
    static bool hasDirectory;
    static string targetDirectory;
    [MenuItem("Assets/Adventure Corp/Create Anim Properties From Selection")]
    private static void CreateAnimClipFromSelectoin()
    {
        foreach (Object o in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(o);
            if (Directory.Exists(path))
            {
                hasDirectory = true;
                targetDirectory = path + "/";
                break;
            }
        }

        Animation a = (Selection.activeObject as GameObject).GetComponent<Animation>();
        bool success = false;
        foreach (AnimationState s in a)
        {
            AnimationClipProperties p = ScriptableObject.CreateInstance<AnimationClipProperties>();
            p.clip = s.clip;
            //p.animatedGameObject = a;

            // Prepeare clip name
            string clipName = s.clip.name;
            clipName = char.ToUpper(clipName[0]) + clipName.Substring(1);            

            // Prepare object name
            string name = Selection.activeGameObject.name;
            name = name.Remove(0, 3);
            name = name.Replace("Agent_", "AP_");            
            name = name + "_" + clipName;

            // Create Assets
            string assetPath = hasDirectory ? targetDirectory : "Assets/";
            assetPath = assetPath + name + ".asset";

            AnimationClipProperties outputAnimClip = AssetDatabase.LoadMainAssetAtPath(assetPath) as AnimationClipProperties;
            if (outputAnimClip != null)
            {
                EditorUtility.CopySerialized(p, outputAnimClip);
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(p, assetPath);
            }
            success = true;            
        }

        if (success)
            Debug.Log("Created " + a.GetClipCount() + " Animation Clip Properties for " + a.name);

        EditorUtility.FocusProjectWindow();
    }


    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Adventure Corp/Create Anim Properties From Selection", true)]
    private static bool CreateAnimClipFromSelectoinValid()
    {
        if (Selection.activeObject.GetType() == typeof(GameObject))
            if ((Selection.activeObject as GameObject).GetComponent<Animation>())
                return true;
        return false;
    }
    // -----------------------------------------------


}
