using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;



public class ModelImportProcessing : AssetPostprocessor
{

    static readonly string MAX_FILE_STRING = "_MAX_";


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
            i.importAnimation = false;
            i.swapUVChannels = false;
        }

        // Settings to import models from 3dsMax
        if (assetPath.Contains(MAX_FILE_STRING))
        {
            i.importMaterials = true;
            i.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            i.materialSearch = ModelImporterMaterialSearch.Everywhere;

            i.importAnimation = false;
            i.animationType = ModelImporterAnimationType.None;
            i.meshCompression = ModelImporterMeshCompression.High;            
        }
        else
        {
            i.importAnimation = true;
            i.importMaterials = false;
            CheckForAnimations(i);
        }
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




    void ScaleMesh(Mesh m, float scaleFactor)
    {

        Vector3[] baseVertices = m.vertices;
        Vector3[] vertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = Vector3.Scale(baseVertices[i], Vector3.one * scaleFactor);

        m.vertices = vertices;
        m.RecalculateBounds();
        
        //       var scale = 1.0;
        //       var recalculateNormals = false;


        //private var baseVertices : Vector3[];


        //function Update()
        //   {
        //       var mesh : Mesh = GetComponent(MeshFilter).mesh;

        //       if (baseVertices == null)
        //           baseVertices = mesh.vertices;

        //       var vertices = new Vector3[baseVertices.Length];

        //       for (var i = 0; i < vertices.Length; i++)
        //       {
        //           var vertex = baseVertices[i];
        //           vertex.x = vertex.x * scale;
        //           vertex.y = vertex.y * scale;
        //           vertex.z = vertex.z * scale;

        //           vertices[i] = vertex;
        //       }

        //       mesh.vertices = vertices;

        //       if (recalculateNormals)
        //           mesh.RecalculateNormals();
        //       mesh.RecalculateBounds();
    }


    void OnPostprocessModel(GameObject gameObject)
    {
        // sets scale 
        // gameObject.transform.localScale = Vector3.one;

        // 2016 Matt - just let it be -90 as it became an ongoing battle with
        // Unity rotating and 3dsMax not, and making sure import/export of 
        // the same mesh was consistent 
        //gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);



        // Some hacking with scale.
        // It is so damn annoying that 'scale factor' in the importer
        // just adds a soft scaling to the gameobject.  The below code
        // literally scales the mesh vertices so that we can just sanely
        // have 1 : 1 units from Max/Unity
        float scaleFactor = 0.0254f;
        if (gameObject.name.Contains(MAX_FILE_STRING))
        {
            foreach (MeshFilter mf in gameObject.GetComponents<MeshFilter>())
            {
                Debug.Log("Scaling for " + mf.name);
                ScaleMesh(mf.sharedMesh, scaleFactor);
            }
            foreach (MeshFilter mf in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                Debug.Log("Scaling for " + mf.name);
                ScaleMesh(mf.sharedMesh, scaleFactor);
            }
        }
            

        GameObject.DestroyImmediate(gameObject.GetComponent<Animator>());
        if (gameObject.GetComponent<Animation>() == null)
            gameObject.AddComponent<Animation>();
    }
}



/*
 
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
*/