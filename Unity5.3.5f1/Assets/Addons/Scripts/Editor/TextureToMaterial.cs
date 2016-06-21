using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureToMaterial
{
    static bool hasDirectory;
    static string targetDir;

    [MenuItem("Assets/Create Material From Texture")]
    private static void DoSomethingWithTexture()
    {
        hasDirectory = false;

        bool keepCheckingDirectories = true;
        foreach(Object o in Selection.objects)
        {
            string selectionPath = AssetDatabase.GetAssetPath(o); // relative path
            if (Directory.Exists(selectionPath) && keepCheckingDirectories)
            {
                keepCheckingDirectories = false;
                hasDirectory = true;
                targetDir = selectionPath;
            }
            else
                CreateMaterial(o as Texture2D);
        }
    }

    private static void CreateMaterial(Texture2D texture)
    {
        //Texture2D tex = Selection.activeObject as Texture2D;
        string path = AssetDatabase.GetAssetPath(texture);
        string fileName = texture.name;

        if (hasDirectory)
        {
            path = targetDir;
        }
        else
        {
            int lastIndex = path.LastIndexOf('/');
            path = path.Substring(0, lastIndex);
        }
        
        // Remove t_ prefix
        if(fileName.Contains("t_"))
        {
            //Debug.Log(fileName.IndexOf("t_"));

            //Debug.Log(fileName.IndexOf("t_"));
            if (fileName.IndexOf("t_") == 0)
            {
                //Debug.Log("here");
                //fileName.Remove(0, 2);
                fileName = fileName.Substring(2);
            }
        }

        Material m = new Material(Shader.Find("Standard"));        
        m.mainTexture = texture;
        AssetDatabase.CreateAsset(m, path + "/m_" + fileName + ".mat");
    }



    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/Create Material From Texture", true)]
    private static bool NewMenuOptionValidation()
    {
        // This returns true when the selected object is a Texture2D (the menu item will be disabled otherwise).
        foreach (Object o in Selection.objects)
            if (o.GetType() == typeof(Texture2D))
                return true;

        return false;
        //return Selection.activeObject.GetType() == typeof(Texture2D);
    }
}