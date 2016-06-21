using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class TestAnimationImport
{
    static bool hasDirectory;
    static string targetDir;

    [MenuItem("Assets/TestAnims")]
    private static void ImportAnimationsFromTextFile()
    {
        //Debug.Log(Selection.objects[0].GetType());

        //TextAsset a = Selection.objects[0] as TextAsset;
        
        //string[] lines = a.text.Split(new [] { '\n' });
        //foreach (string s in lines)
        //    Debug.Log(s);

        //Debug.Log(a.text);       

        //ExtractAnimsFromTxt((Selection.objects[0] as TextAsset).text);
    }

    static void ExtractAnimsFromTxt(string text)
    {
        string[] lines = text.Split('\n');
        List<string> validFrames = new List<string>();
        
        foreach (string s in lines)
        {
            if (s.Contains("frame:"))            
                validFrames.Add(s);                
        }        

        //string[] p = validFrames[0].Split(null);
        //foreach (string s in p)
        //    Debug.Log(s);

        foreach (string s in validFrames)
        {
            string[] elements = s.Split(null);
            Debug.Log(elements.Length);
            foreach (string e in elements)
                Debug.Log(e);
        }
    }



    // Note that we pass the same path, and also pass "true" to the second argument.
    [MenuItem("Assets/TestAnims", true)]
    private static bool NewMenuOptionValidation()
    {
        Debug.Log(Selection.objects[0].GetType());
        return true;
        //return Selection.objects[0].GetType() == typeof(TextAsset);
    }
}