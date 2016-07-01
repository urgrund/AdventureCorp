using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AdventureCorpEditorHelpers
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
                Debug.Log("Will create files in " + targetDirectory);
                break;
            }
        }

        Animation a = (Selection.activeObject as GameObject).GetComponent<Animation>();
        foreach (AnimationState s in a)
        {
            AnimationClipProperties p = ScriptableObject.CreateInstance<AnimationClipProperties>();
            p.clip = s.clip;           

            // Prepeare clip name
            string clipName = s.clip.name;

            clipName = char.ToUpper(clipName[0]) + clipName.Substring(1);

            //clipName = clipName.First().ToString().ToUpper() + System.String.Join("", clipName.Skip(1));

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
            //    //outputAnimClip = new AnimationClipProperties();
            //    //EditorUtility.CopySerialized(animClip, outputAnimClip);
                AssetDatabase.CreateAsset(p, assetPath);
            }


            //AssetDatabase.CreateAsset(p, assetPath);
            //AssetDatabase.SaveAssets();
        }

        EditorUtility.FocusProjectWindow();
        

        //MyScriptableObjectClass asset = ScriptableObject.CreateInstance<MyScriptableObjectClass>();

        //AssetDatabase.CreateAsset(asset, "Assets/NewScripableObject.asset");
        //AssetDatabase.SaveAssets();

        //EditorUtility.FocusProjectWindow();

        //Selection.activeObject = asset;
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
