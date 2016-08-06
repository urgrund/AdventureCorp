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
                // Self create test
                //Debug.LogWarning("No Level Manager was found in scene, creating at runtime");
                //GameObject go = new GameObject("p_LevelManager");
                //_instance = go.AddComponent<LevelManager>();                
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

    void Awake()
    {
        _instance = this;
        Application.targetFrameRate = 60;
		Random.seed = (int)System.DateTime.Now.Ticks;
	}

    public static void Register(Health h)
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

    public static void DeRegister(Health h)
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
}
