using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance = null; // LevelManager is a singleton

    
    public static LevelManager instance
    {
        get
        {
            if (_instance == null)
            {
				// Does this need to be a monobehaviour?           
            }
            return _instance;
        }
    }
	

    private List<Health> _healthObjects = new List<Health>(); // All objects with a health script are found here
    public static List<Health> healthObjects{ get { return _instance._healthObjects; } }

    private List<Health> _NPCs = new List<Health>();  //All NPCs with a health script are found here
    public static List<Health> NPCs{ get { return _instance._NPCs; } }

    private List<Health> _players = new List<Health>(); //All Players with a health script are found here
    public static List<Health> players { get { return _instance._players; } }

    private List<Health> _other = new List<Health>(); // All other objects with a health script are found here
    public static List<Health> other { get { return _instance._other; } }

    void OnEnable()
    {
        _instance = this;
        Application.targetFrameRate = 60;
		Random.InitState((int)System.DateTime.Now.Ticks);

		UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneWasUnLoaded;
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneWasLoaded;
	}


	void OnSceneWasLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
	{

	}

	void OnSceneWasUnLoaded(UnityEngine.SceneManagement.Scene scene)
	{	
	
	}


    public static void RegisterHealth(Health h)
    {
        if (_instance == null)
        {
            Debug.LogWarning("No Level Manager In Scene");
            return;
        }

        _instance._healthObjects.Add(h);

        if (h.GetComponent<ExplorerBrain>() != null)
        {
            _instance._players.Add(h);
        }
        else if (h.GetComponent<NPCBrain>() != null)
        {
            _instance._NPCs.Add(h);
        }
        else
        {
            _instance._other.Add(h);
        }
    }

    public static void DeRegisterHealth(Health h)
    {
        if (_instance == null)
        {
            Debug.LogWarning("No Level Manager In Scene");
            return;
        }

        _instance._healthObjects.Remove(h);

        if (h.GetComponent<ExplorerBrain>() != null)
        {
            _instance._players.Remove(h);
        }
        else if (h.GetComponent<NPCBrain>() != null)
        {
            _instance._NPCs.Remove(h);
        }
        else
        {
            _instance._other.Remove(h);
        }
    }









	// -----------------------------------------------------------------------------------------------------------
	// 
	//	Editor Only 
	//



	public bool isDrawSelectedNormals = false;
	public GameObject selectedGameObject = null;
	public float selectedNormalsLength = 1f;

	private MeshFilter[] _selectedMeshFilters = null;

	void EditorDrawSelectedNormals()
	{
		if (isDrawSelectedNormals && selectedGameObject != null)
		{
			Gizmos.color = Color.magenta;

			if (_selectedMeshFilters == null)
				_selectedMeshFilters = selectedGameObject.GetComponentsInChildren<MeshFilter>();

			foreach (MeshFilter mf in _selectedMeshFilters)
			{
				for (int i = 0; i < mf.sharedMesh.vertexCount; i++)
				{
					Vector3 p = selectedGameObject.transform.TransformPoint(mf.sharedMesh.vertices[i]);
					Vector3 n = selectedGameObject.transform.TransformDirection(mf.sharedMesh.normals[i]);
					Gizmos.DrawRay(p, n * selectedNormalsLength);
				}
			}
		}
		else
			_selectedMeshFilters = null;
	}





	public bool isDrawEditorColliders;
	public float editorColliderColorAlpha = 0.25f;

	Color colliderWireColor = Color.red;
	Color colliderTriggerColor = Color.yellow;
	BoxCollider[] allBoxColliders;
	SphereCollider[] allSphereColliders;
	MeshCollider[] allMeshColliders;

	public void EditorCollectionAllColliders()
	{
		allBoxColliders = FindObjectsOfType<BoxCollider>();
		allSphereColliders= FindObjectsOfType<SphereCollider>();
		allMeshColliders = FindObjectsOfType<MeshCollider>();
	}

		

	void EditorDrawColliderGizmos()
	{
		if (isDrawEditorColliders)
		{
			Color cA = colliderWireColor * new Color(1, 1, 1, editorColliderColorAlpha);
			Color cAT = colliderTriggerColor * new Color(1, 1, 1, editorColliderColorAlpha);
			if (allBoxColliders != null)
			{
				foreach (BoxCollider c in allBoxColliders)
				{
					Gizmos.matrix = c.transform.localToWorldMatrix;
					Gizmos.color = c.isTrigger ? colliderTriggerColor : colliderWireColor;
					Gizmos.DrawWireCube(c.center, c.size);
					Gizmos.color = c.isTrigger ? cAT: cA;
					Gizmos.DrawCube(c.center, c.size);
				}
			}
			if (allSphereColliders != null)
			{
				foreach (SphereCollider c in allSphereColliders)
				{
					Gizmos.matrix = c.transform.localToWorldMatrix;
					Gizmos.color = c.isTrigger ? colliderTriggerColor : colliderWireColor;
					Gizmos.DrawWireSphere(c.center, c.radius);
					Gizmos.color = c.isTrigger ? cAT : cA;
					Gizmos.DrawSphere(c.center, c.radius);					
				}
			}
			if (allMeshColliders != null)
			{
				foreach (MeshCollider c in allMeshColliders)
				{
					Gizmos.matrix = c.transform.localToWorldMatrix;
					Gizmos.color = c.isTrigger ? colliderTriggerColor : colliderWireColor;
					Gizmos.DrawWireMesh(c.sharedMesh);
					Gizmos.color = c.isTrigger ? cAT : cA;
					Gizmos.DrawMesh(c.sharedMesh); 
				}
			}
		}
	}


	void OnDrawGizmos()
	{	
		EditorDrawColliderGizmos();
		EditorDrawSelectedNormals();
	}

	// -----------------------------------------------------------------------------------------------------------
}
