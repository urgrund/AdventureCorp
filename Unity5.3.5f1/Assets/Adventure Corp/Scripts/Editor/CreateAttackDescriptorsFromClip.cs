using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;


public class CreateAttackDescriptorsFromClip
{

    // -----------------------------------------------
    // Create anim properties
    static bool hasDirectory;
    static string targetDirectory;
    
    [MenuItem("Assets/Adventure Corp/Create Attack Descriptors From Clips")]    
    private static void CreateAttackDescriptors()
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
        
        
        foreach(Object o in Selection.objects)
        {
            if (o.GetType() != typeof(AnimationClipProperties))
                continue;

            AnimationClipProperties cp = o as AnimationClipProperties;
                    
            AttackDescriptor ad = ScriptableObject.CreateInstance<AttackDescriptor>();
            ad.clipProperties = cp;

            // Add volumes from agent if available 
            if (cp.agent != null && cp.agent.properties != null)
            {
                if (cp.agent.properties.GetComponent<AttackVolumeCollection>())
                    ad.volumeIndices = new bool[cp.agent.properties.GetComponent<AttackVolumeCollection>().volumes.Length];
            }

            // Prepare object name
            string name = cp.name;
            name = name.Remove(0, 3);
            name = AdventureCorpGlobals.Editor.Prefixes.AttackDescriptor + name;
            
            //// Create Assets
            string assetPath = hasDirectory ? targetDirectory : "Assets/";
            assetPath = assetPath + name + ".asset";

            AttackDescriptor outPutObject = AssetDatabase.LoadMainAssetAtPath(assetPath) as AttackDescriptor;
            if (outPutObject != null)
            {
                EditorUtility.CopySerialized(ad, outPutObject);
                AssetDatabase.SaveAssets();
            }
            else
                AssetDatabase.CreateAsset(ad, assetPath);
        }        

        EditorUtility.FocusProjectWindow();
    }

    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Adventure Corp/Create Attack Descriptors From Clips", true)]
    private static bool CreateAttackDescriptorsValid()
    {
        foreach (Object o in Selection.objects)
        {
            if (o.GetType() == typeof(AnimationClipProperties))
                return true;
        }
         //       return (Selection.activeObject.GetType() == typeof(AnimationClipProperties));
            //return true;
        
        return false;
    }
    // -----------------------------------------------


}
