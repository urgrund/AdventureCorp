using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public LevelManager levelManager;
    public PatrolManager patrolManager;

    void Awake()
    {
        Instantiate(levelManager);
        Instantiate(patrolManager);
    }
}
