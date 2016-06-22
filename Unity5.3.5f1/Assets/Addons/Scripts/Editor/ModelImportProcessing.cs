using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;



public class ModelImportProcessing : AssetPostprocessor
{

    void OnPreprocessModel()
    {
        ModelImporter i = (assetImporter as ModelImporter);
        //i.generateMaterials = ModelImporterGenerateMaterials.PerSourceMaterial;
        //if (!(assetPath.Contains("Anim")))
        {
            //i.globalScale = 0.0254f;
            //  i.importAnimation = false;
            //i.generateAnimations = ModelImporterGenerateAnimations.None;
            //i.meshCompression = ModelImporterMeshCompression.High;
            i.importMaterials = false;
            i.swapUVChannels = false;
        }

        CheckForAnimations(i);
    }



    void CheckForAnimations(ModelImporter assetImporter)
    {
        // Get the actual name of the asset
        // Can't unity doesn't just provide this :(
        int trimExt = 4; // 4 = ".fbx" 
        //int start = assetImporter.assetPath.LastIndexOf('/')+1;
        //int end = assetImporter.assetPath.Length - trimExt - start;
        //string assetName = assetImporter.assetPath.Substring(start, end);
        //Debug.Log(assetName);

        string testPath = assetPath;
        testPath = testPath.Substring(0, testPath.Length - trimExt);
        testPath += ".txt";
        //Debug.Log(testPath);
        foreach (TextAsset t in Resources.FindObjectsOfTypeAll<TextAsset>())
        {
            //Debug.Log(AssetDatabase.GetAssetPath(t));
            //Debug.Log(testPath.Equals(AssetDatabase.GetAssetPath(t).ToString()));
            //Debug.Log(AssetDatabase.GetAssetPath(t) == testPath);
            //Debug.Log(string.Compare(AssetDatabase.GetAssetPath(t), testPath) == 0);

            if (AssetDatabase.GetAssetPath(t).Equals(testPath))
            {
                //Debug.Log("Found! " + t.name);
                assetImporter.clipAnimations = ExtractAnimsFromTxt(t.text).ToArray();
                break;
            }
        }
    }


    static List<ModelImporterClipAnimation> ExtractAnimsFromTxt(string text)
    {
        List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();

        string[] lines = text.Split('\n');
        List<string> validFrames = new List<string>();

        // Find valid animation frame listing in 
        // the text file as they start with 'frame:'
        foreach (string s in lines)
        {
            if (s.ToLower().Contains("frame:"))
                validFrames.Add(s);
        }

        // For each valid frame string, split it
        // into its components which is 2 ints for
        // start and end times, and a string for name
        foreach (string s in validFrames)
        {
            string[] elements = s.Split(null);
            string[] times = elements[1].Split(new char[] { '-', ':' });  //('-');
            int start, end;

            //for(int i=0; i<times.Length;i++)

            start = int.Parse(times[0]);
            end = int.Parse(times[1]);

            Debug.Log("Name " + elements[2]);
            Debug.Log("s " + start + "   e " + end);

            ModelImporterClipAnimation c = new ModelImporterClipAnimation();
            c.firstFrame = start;
            c.lastFrame = end;
            c.name = elements[2];
            clips.Add(c);
        }

        return clips;
    }






    void OnPostprocessModel(GameObject gameObject)
    {
        // sets scale and rotation 
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);


        // Materials
        // For each material, find it's 'real' name then see if it exists in 
        // resources and set this as the material        
        if (gameObject.transform.GetComponent(typeof(Renderer)) != null)
        {
            Material[] mats = gameObject.transform.GetComponent<Renderer>().sharedMaterials;
            if (gameObject.transform.GetComponent(typeof(MeshRenderer)) != null)
            {
                //int len = gameObject.name.Length + 1;
                for (int i = 0; i < mats.Length; i++)
                {
                    //Material m = mats[i];
                    //string matName = m.name.Substring(len, m.name.Length - len);
                    foreach (Object o in Resources.FindObjectsOfTypeAll(typeof(Material)))
                        if ((o as Material).name == mats[i].name) //matName)
                            mats[i] = (o as Material);
                }
                gameObject.transform.GetComponent<Renderer>().sharedMaterials = mats;
            }
        }

        GameObject.DestroyImmediate(gameObject.GetComponent<Animator>());

        if (gameObject.GetComponent<Animation>() == null)
            gameObject.AddComponent<Animation>();
    }
}



