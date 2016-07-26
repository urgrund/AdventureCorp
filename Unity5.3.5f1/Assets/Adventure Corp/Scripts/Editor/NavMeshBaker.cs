﻿// NavigationHelper - Allows you to create Unity Nav Meshes using Box Colliders - 2014-01-09
// released under MIT License
// http://www.opensource.org/licenses/mit-license.php
//
//@author		Devin Reimer - Owlchemy Labs
//@website 		http://blog.almostlogical.com
/*
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

/*
 * Usage: To use set COLLIDER_LAYER const (below) to whatever layer you wish for the Box Colliders to be used from. This allows you to use some box colliders and
 *        not others. If you don't change it (-1) it will use all layers.
 *        You will also need to create a new Tag (Edit->Project Settings->Tags) name this tag: TempNavMeshItemDestroyable
 */

//Note: This class uses UnityEditorInternal which is an undocumented internal feature, it uses it to make this script less error prone
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NavMeshBaker : EditorWindow
{
    private int COLLIDER_LAYER = 8; //edit this valve to set which layer a box collider needs to be on to be read

    private const string TEMP_NAV_MESH_OBJECT_TAG = "TempNavMeshItemDestroyable"; //you can change to what ever tag you would like as long as it isn't used by anything else
    private bool isSetup = true;

    private static List<GameObject> navMeshPrefabs;
    private static bool isBuildLightProbes = true;
    private static string LIGHT_PROBE_TAG = "LightProbeContainer";



    [MenuItem("Window/Adventure Corp/NavMesh Baker")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(NavMeshBaker));
    }

    //return true if exists
    private bool CheckIfTagExists()
    {
        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
        for (int n = 0; n < tags.Length; n++)
        {
            if (tags[n] == TEMP_NAV_MESH_OBJECT_TAG)
            {
                return true;
            }
        }
        return false;
    }


    void OnGUI()
    {
        GUILayout.Label("Build Nav Mesh Including Custom Box Colliders", EditorStyles.boldLabel);

        if (!isSetup)
        {
            isSetup = CheckIfTagExists();
            if (!isSetup)
            {
                GUILayout.Label("Error - You first need to create a Tag called:");
                EditorGUILayout.TextArea(TEMP_NAV_MESH_OBJECT_TAG);
            }
        }

        if (isSetup)
        {
            COLLIDER_LAYER = EditorGUILayout.LayerField(COLLIDER_LAYER);
            if (GUILayout.Button("Build Nav Mesh!"))
            {
                if (CheckIfTagExists())
                {
                    BakeColliders();
                }
                else
                {
                    Debug.LogError("Custom tag was not created");
                    isSetup = false;
                }
            }
            isBuildLightProbes = EditorGUILayout.Toggle("Light Probes", isBuildLightProbes);
        }
     }


    

    private void BakeColliders()
    {
        CleanUpOldNavMeshItems();	
        navMeshPrefabs = new List<GameObject>();
        SetupBoxes();
        SetupCapsules();
        SetupSpheres();
        SetupMeshColliders();
        NavMeshBuilder.BuildNavMeshAsync();
        CleanUpOldNavMeshItems();
        GenerateLightProbes();
    }




    void GenerateLightProbes()
    {
        NavMeshTriangulation t = NavMesh.CalculateTriangulation();

        GameObject go;
        LightProbeGroup lpg;
        if (GameObject.FindGameObjectWithTag(LIGHT_PROBE_TAG))
        {
            go = GameObject.FindGameObjectWithTag(LIGHT_PROBE_TAG);
            lpg = go.GetComponent<LightProbeGroup>();
            Debug.Log("Probe container extist...  updating.");
        }
        else
        {
            go = new GameObject("LIGHT_PROBES");
            go.tag = LIGHT_PROBE_TAG;
            lpg = go.AddComponent<LightProbeGroup>();
        }

        go.isStatic = true;

        List<Vector3> points = new List<Vector3>();
        foreach (Vector3 p in t.vertices)
        {
            points.Add(p + Vector3.up * 0.5f);
            points.Add(p + Vector3.up * AdventureCorpGlobals.Editor.NavMesh.agentHeight * 1.2f);
        }
        lpg.probePositions = points.ToArray(); 
    }




    void SetupBoxes()
    {
        BoxCollider[] allColliders = GameObject.FindObjectsOfType<BoxCollider>();
        GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PreparePrefab(ref tempGO);

        GameObject tempNavMeshCube;
        foreach (BoxCollider c in allColliders)
        {
            if (COLLIDER_LAYER < 0 || c.gameObject.layer == COLLIDER_LAYER)
            {
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center);
                tempNavMeshCube.transform.localScale = c.size;
            }
        }
        DestroyImmediate(tempGO);
    }


    void SetupCapsules()
    {
        CapsuleCollider[] allColliders = GameObject.FindObjectsOfType<CapsuleCollider>();
        GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        PreparePrefab(ref tempGO);

        GameObject tempNavMeshCube;
        foreach (CapsuleCollider c in allColliders)
        {
            if (COLLIDER_LAYER < 0 || c.gameObject.layer == COLLIDER_LAYER)
            {
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center);
                tempNavMeshCube.transform.localScale = new Vector3(c.radius * 2f, c.height / 2f, c.radius * 2f);
            }
        }
        DestroyImmediate(tempGO);
    }


    void SetupSpheres()
    {
        SphereCollider[] allColliders = GameObject.FindObjectsOfType<SphereCollider>();
        GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        PreparePrefab(ref tempGO);

        GameObject tempNavMeshCube;
        foreach (SphereCollider c in allColliders)
        {
            if (COLLIDER_LAYER < 0 || c.gameObject.layer == COLLIDER_LAYER)
            {
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center);
                tempNavMeshCube.transform.localScale = Vector3.one * c.radius * 2f;
            }
        }
        DestroyImmediate(tempGO);
    }

    void SetupMeshColliders()
    {
        MeshCollider[] allColliders = GameObject.FindObjectsOfType<MeshCollider>();
        GameObject tempGO = new GameObject(TEMP_NAV_MESH_OBJECT_TAG);
        tempGO.AddComponent<MeshFilter>();
        tempGO.AddComponent<MeshRenderer>();
        PreparePrefab(ref tempGO);

        GameObject tempNavMeshCube;
        foreach (MeshCollider c in allColliders)
        {
            if (COLLIDER_LAYER < 0 || c.gameObject.layer == COLLIDER_LAYER)
            {
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, Vector3.zero);
                tempNavMeshCube.GetComponent<MeshFilter>().mesh = c.sharedMesh;
                tempNavMeshCube.transform.localScale = Vector3.one;
            }
        }
        DestroyImmediate(tempGO);
    }




    void PreparePrefab(ref GameObject g)
    {
        DestroyImmediate(g.GetComponent<Collider>());
        g.name = TEMP_NAV_MESH_OBJECT_TAG;
        GameObjectUtility.SetStaticEditorFlags(g, StaticEditorFlags.NavigationStatic);
        g.tag = TEMP_NAV_MESH_OBJECT_TAG;
    }

    void PrepareTempNavMeshObject(out GameObject g, GameObject tempGO, Transform colliderTransform, Vector3 colliderCenter)
    {
        g = Instantiate(tempGO) as GameObject;
        g.name = tempGO.name;
        g.transform.parent = colliderTransform;
        g.transform.localPosition = colliderCenter;
        g.transform.localRotation = Quaternion.identity;
        g.hideFlags = HideFlags.DontSave;
        navMeshPrefabs.Add(g);
    }



    private void GenerateWayPoints()
    {
    }



    private void CleanUpOldNavMeshItems()
    {
        if (navMeshPrefabs != null)
        {
            foreach (GameObject go in navMeshPrefabs)
                DestroyImmediate(go);
        }
        navMeshPrefabs = null;
        //GameObject[] oldNavMeshItems = GameObject.FindGameObjectsWithTag(TEMP_NAV_MESH_OBJECT_TAG);
        //foreach (GameObject go in oldNavMeshItems)
        //{
        //    DestroyImmediate(go);
        //}
    }
}