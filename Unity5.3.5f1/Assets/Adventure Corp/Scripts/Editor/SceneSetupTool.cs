



//Note: This class uses UnityEditorInternal which is an undocumented internal feature, it uses it to make this script less error prone
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SceneSetupTool : EditorWindow
{
	private const string TEMP_NAV_MESH_OBJECT_TAG = "TempNavMeshItemDestroyable"; //you can change to what ever tag you would like as long as it isn't used by anything else
	private bool isSetup = true;

	private static string LIGHT_PROBE_TAG = "LightProbeContainer";

	
	//SceneSetupToolVisualisation tempVizObj = null;

	[MenuItem("Window/Adventure Corp/Scene Tools %W")]
	static void Init()
	{
		EditorWindow.GetWindow(typeof(SceneSetupTool));
	}


	//void OnFocus()
	//{		
	//	SceneView.onSceneGUIDelegate -= this.OnSceneGUI;		
	//	SceneView.onSceneGUIDelegate += this.OnSceneGUI;		
	//}

	void OnEnable()
	{	
		levelManagerInScene = GameObject.FindObjectOfType<LevelManager>();
		if (levelManagerInScene != null)		
			GetAllCollisionShapes();		
	}
	
	void GetAllCollisionShapes()
	{
		if (levelManagerInScene != null)		
			levelManagerInScene.EditorCollectionAllColliders();		
	}
	


	void OnHierarchyChange()
	{
		GetAllCollisionShapes();
	}

	LevelManager levelManagerInScene = null;

	float btnHeight = 50;
	bool _lastIsDraw = true;
	void OnGUI()
	{
		

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (GUILayout.Button("Build Nav Mesh!", GUILayout.Height(btnHeight)))
			BakeNavMeshUsingVisualMesh();
		
		EditorGUILayout.Space();
		if (GUILayout.Button("Create Light Probes On NavMesh", GUILayout.Height(btnHeight)))
			GenerateLightProbes();

		if (FindObjectOfType<LevelManager>() == null)
		{
			EditorGUILayout.Space();
			if (GUILayout.Button("Create Scene Directories", GUILayout.Height(btnHeight)))
				NewSceneSetup();
		}


		EditorGUI.BeginChangeCheck();
		if (levelManagerInScene != null)
		{
			levelManagerInScene.isDrawEditorColliders = EditorGUILayout.Toggle("Draw Colliders", levelManagerInScene.isDrawEditorColliders);
			levelManagerInScene.editorColliderColorAlpha = EditorGUILayout.Slider("Collider Alpha", levelManagerInScene.editorColliderColorAlpha, 0, 1);


			// Draw normals
			EditorGUILayout.Space();
			levelManagerInScene.isDrawSelectedNormals = EditorGUILayout.Toggle("Render Normals", levelManagerInScene.isDrawSelectedNormals);
			levelManagerInScene.selectedNormalsLength = EditorGUILayout.Slider("Length", levelManagerInScene.selectedNormalsLength, 0, 1);
			if (Selection.activeGameObject != null)
			{
				levelManagerInScene.selectedGameObject = Selection.activeGameObject;
				EditorGUILayout.LabelField("Selected Properties", EditorStyles.boldLabel);

				int verts = 0;
				int tris = 0;
				foreach (GameObject go in Selection.gameObjects)
				{
					foreach (MeshFilter m in go.GetComponentsInChildren<MeshFilter>())
					{
						if (m == null)
							continue;
						if (m.sharedMesh == null)
							continue;
						verts += m.sharedMesh.vertexCount;
						tris += m.sharedMesh.triangles.Length;
					}
				}
				EditorGUILayout.LabelField("Vertices  : " + verts);
				EditorGUILayout.LabelField("Tris      : " + tris);
				//EditorGUILayout.LabelField("UV's      : " + 

			}
			else
				levelManagerInScene.selectedGameObject = null;
		}


		// If changes, refresh scene
		if (EditorGUI.EndChangeCheck())
			SceneView.RepaintAll();		
	}






	void NewSceneSetup()
	{
		new GameObject(AdventureCorpGlobals.Editor.SceneFolderNames.MANAGERS);
		new GameObject(AdventureCorpGlobals.Editor.SceneFolderNames.PREFAB_FOLDER);
		new GameObject(AdventureCorpGlobals.Editor.SceneFolderNames.LIGHTS_FOLDER);
		new GameObject(AdventureCorpGlobals.Editor.SceneFolderNames.NPC_SPAWN);
	}










	// -------------------------------------------------------------------------------------------------------
	// -------------------------------------------------------------------------------------------------------
	// Build Light Probes
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
	// -------------------------------------------------------------------------------------------------------
	// -------------------------------------------------------------------------------------------------------


















	// -------------------------------------------------------------------------------------------------------
	// -------------------------------------------------------------------------------------------------------
	// Build Nav Mesh
	// NavigationHelper - Allows you to create Unity Nav Meshes using Box Colliders - 2014-01-09
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



	private static List<GameObject> tempNavMeshPrefabs;
	private static List<GameObject> navMeshPrefabs;

	private struct SourceObjectNavBake
	{
		public GameObject source;
		public bool wasActive;
	}
	private static List<SourceObjectNavBake> navMeshPrefabSources;


	private void BakeNavMeshUsingVisualMesh()
	{
		CleanUpOldNavMeshItems();
		navMeshPrefabs = new List<GameObject>();
		tempNavMeshPrefabs = new List<GameObject>();
		navMeshPrefabSources = new List<SourceObjectNavBake>();
		Debug.Log("Baking NavMesh");
		//Debug.Log("-- Following Objects were used --");

		SetupBoxes();
		SetupCapsules();
		SetupSpheres();
		SetupMeshColliders();
		PrepareDeactivation();
		NavMeshBuilder.BuildNavMeshAsync();
		CleanUpOldNavMeshItems();
	}


	void PrepareDeactivation()
	{
		// For each collider we create a nav mesh object 
		// from, we need to de-activate the original 
		// otherwise it is included in the bake
		foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
		{
			foreach (Transform t in go.GetComponentsInChildren<Transform>())
			{
				SourceObjectNavBake s = new SourceObjectNavBake();
				s.source = t.gameObject;
				s.wasActive = t.gameObject.activeInHierarchy || t.gameObject.activeSelf;
				s.source.SetActive(false);
				navMeshPrefabSources.Add(s);
			}
		}
		//Debug.Log("Set all to inactive (" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Length + ")");

		foreach (GameObject t in navMeshPrefabs)
			t.SetActive(true);
		//Debug.Log("Set colliders to active (" + navMeshPrefabs.Count + ")");
	}



	void SetupBoxes()
    {
        BoxCollider[] allColliders = GameObject.FindObjectsOfType<BoxCollider>();
        GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PreparePrefab(ref tempGO);

        GameObject tempNavMeshCube;
        foreach (BoxCollider c in allColliders)
        {
			if (c.isTrigger)
				continue;
							
			if(GameObjectUtility.AreStaticEditorFlagsSet(c.gameObject, StaticEditorFlags.NavigationStatic))
			{
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center, c.name);
				tempNavMeshCube.transform.localScale = Vector3.Scale(c.transform.localScale, c.size); 
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
			if (c.isTrigger)
				continue;
			
			if (GameObjectUtility.AreStaticEditorFlagsSet(c.gameObject, StaticEditorFlags.NavigationStatic))
			{
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center, c.name);
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
			if (c.isTrigger)
				continue;
			
			if (GameObjectUtility.AreStaticEditorFlagsSet(c.gameObject, StaticEditorFlags.NavigationStatic))
			{
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, c.center, c.name);
                tempNavMeshCube.transform.localScale = c.transform.localScale * c.radius * 2f;
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
			if (c.isTrigger)
				continue;
			
			if (GameObjectUtility.AreStaticEditorFlagsSet(c.gameObject, StaticEditorFlags.NavigationStatic))
			{
                PrepareTempNavMeshObject(out tempNavMeshCube, tempGO, c.transform, Vector3.zero, c.name);
				//Debug.Log(c.sharedMesh.name);
                tempNavMeshCube.GetComponent<MeshFilter>().mesh = c.sharedMesh;
				tempNavMeshCube.transform.localScale = c.gameObject.transform.localScale; // Vector3.one;
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

    void PrepareTempNavMeshObject(out GameObject g, GameObject tempGO, Transform colliderTransform, Vector3 colliderCenter, string goName="")
    {
        g = Instantiate(tempGO) as GameObject;
        g.name = tempGO.name;

		g.transform.parent = colliderTransform;
		g.transform.localPosition = colliderCenter;
		g.transform.localRotation = Quaternion.identity;
		g.transform.parent = null;

		//g.transform.localPosition = colliderTransform.position + colliderCenter;
		//g.transform.rotation = colliderTransform.rotation;
		//g.transform.localScale = colliderTransform.localScale;


        //g.hideFlags = HideFlags.DontSave;

		//Debug.Log("  - " + goName);
		navMeshPrefabs.Add(g);
		tempNavMeshPrefabs.Add(colliderTransform.gameObject);

	}
	
    private void CleanUpOldNavMeshItems()
    {
		if (navMeshPrefabSources != null)
			foreach (SourceObjectNavBake n in navMeshPrefabSources)
				n.source.SetActive(n.wasActive);

		if (navMeshPrefabs != null)
			foreach (GameObject go in navMeshPrefabs)
				DestroyImmediate(go);

		navMeshPrefabSources = null;
        navMeshPrefabs = null;
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
}