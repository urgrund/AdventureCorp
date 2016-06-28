﻿using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
/// <summary>
/// NPC Brain
/// NPC Brains will work on a concept of a desired destination
/// This emulated how we think such as 'I want to get to that door'
/// So rather than the brain controlling every step (that's the agent/controller)
/// the NPC Brain merely figures out a destination.  Part of the 'intelligence' is
/// figuring out a path and this will be done here as well which delegates the Agent
/// as simply being a 'vehicle' thorugh which to move discreetly
/// </summary>
public abstract class NPCBrain : Brain
{
    // Callbacks for the Brain arriving at positions of interest
    public delegate void ArrivedAtDestination();
    public event ArrivedAtDestination onArrivedAtDestination;
    public event ArrivedAtDestination onArrivedAtNavMeshPosition;

    public AIProperties properties;

    //---------------------------------------------------------------------------------------------------------------//
    //Hostile targets variables and functions
    Transform hostileTarget; //Behviour of AI can change if hostileTarget is not null
    List<Transform> potentialHostileTargets = new List<Transform>(); //AI will look for hostile targets in this array

    public HostileTargets hostileTargets; //Determines which objects count as a hostile target (Player, NPC or both)

    // Alert details that are specific 
    // to the brain/character
    // Callbacks for the Brain arriving at positions of interest
    public delegate void HostileTargetSpotted();
    public event HostileTargetSpotted onHostileTargetSpotted; //This event fires off when an enemy spots a hostile target
    public delegate void HostileTargetCleared();
    public event HostileTargetCleared onHostileTargetCleared; //This event fires off when an enemy does not have a hostile target anymore

    public float alertDistanceFar = 5; // How far ahead can the agent detect hostile targets
    public float alertDistanceFarCone = 45; // In what angle can the angent spot infront of him
    public float alertDistanceNear = 1; // How close can the agent detect hostile targets without looking in the cone

    //TODO: How to avoid NPC detecting player if he is behind walls or above him in the y direction 
    bool HostileTargetInFarRange(Transform t) { return (Vector3.Distance(t.position, transform.position) < alertDistanceFar); }
    bool HostileTargetInNearRange(Transform t) { return (Vector3.Distance(t.position, transform.position) < alertDistanceNear);}
    bool HostileTargetInFarConeAndDistance(Transform t) { return HostileTargetInFarRange(t) && HostileTargetInFarCone(t); }
    bool HostileTargetInFarCone(Transform t)
    {
        Vector3 dirToTarget = (t.position - transform.position).normalized;
        return (Vector3.Angle(dirToTarget, transform.forward) < alertDistanceFarCone * 0.5f);
    }

    /// <summary>
    /// Finds if a potential hostile target is in any range of the AI and sets it as the hostile target
    /// </summary>
    public bool HostileTargetInAnyRange()
    {
        if (potentialHostileTargets.Count == 0)
            return false;

        for (int i = 0; i < potentialHostileTargets.Count; i++)
        {
            if(HostileTargetInNearRange(potentialHostileTargets[i]) || HostileTargetInFarConeAndDistance(potentialHostileTargets[i]))
            {
                //TODO store all potential hostile targets in an array and then check which one is the closest one and pick that one as a hostile target
                SetHostileTarget(potentialHostileTargets[i]);
                return true;
            }
        }
        return false;
    }


    public void SetHostileTarget(Transform t)
    {
        hostileTarget = t;

        if (onHostileTargetSpotted != null)
            onHostileTargetSpotted();
    }

    public void RemoveHostileTarget()
    {
        if (hostileTarget == null)
            return;

        hostileTarget = null;

        if (onHostileTargetCleared != null)
            onHostileTargetCleared();
    }
    //-------------------------------------------------------------------------------------------------------------------//
    
    public bool ShowGizmoz = true;
    List<PatrolPoint> myPatrolPoints = new List<PatrolPoint>();

    protected virtual void OnArrivedAtDestination()
    {   
        _destination = null;
        _isArrivedAtDestination = true;
    }

    protected virtual void OnArrivedAtNavMeshPosition()
    {
        if (_navMeshNextPositionIndex < (_navMeshPathToDestination.corners.Length - 1))
        {
            _navMeshNextPositionIndex++;
            _navMeshNextPosition = _navMeshPathToDestination.corners[_navMeshNextPositionIndex];

            if(_navMeshNextPositionIndex == _navMeshPathToDestination.corners.Length)
                _isNavMeshPositionFinal = true;
        }        
    }


    // TODO - this should go into a properties container?
    private float _destinationBias = 1f;

    public NavMeshPath _navMeshPathToDestination;
    public Vector3? _navMeshNextPosition;     
    public int _navMeshNextPositionIndex = 0;
    public bool _isNavMeshPositionFinal = false;

    protected bool _isArrivedAtDestination = false;
    protected Vector3? _destination = null;
    public Vector3? destination
    {
        set
        {
            // If already there, exit
            if (CheckAtPosition((Vector3)value, _destinationBias))
                return;
            
            // Otherwise setup path to the new destination             
            FindPathToPosition((Vector3)value, false);
        }

        get
        {
            return _destination;
        }
    }

    protected void FindPathToPosition(Vector3 position) { FindPathToPosition(position, false); }  
    protected void FindPathToPosition(Vector3 position, bool addToCurrentPath)
    {
        _isNavMeshPositionFinal = false;
        _isArrivedAtDestination = false;
        _navMeshPathToDestination = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, _navMeshPathToDestination))
        {
            _navMeshNextPositionIndex = 1;
            _navMeshNextPosition = _navMeshPathToDestination.corners[_navMeshNextPositionIndex];
            _destination = _navMeshPathToDestination.corners[_navMeshPathToDestination.corners.Length - 1];
        }
        else
        {
            _destination = position;
            _navMeshNextPosition = null;
        }
    }


    public Vector3 directionToDestination { get { return ((Vector3)_destination - transform.position).normalized; } }
    public Vector3 DirectionToPosition(Vector3 position) { return (position - transform.position).normalized; }
    public bool CheckAtPosition(Vector3 position) { return CheckAtPosition(position, 0.01f); }
    public bool CheckAtPosition(Vector3 position, float bias) { return Helpers.InRadius(transform.position, position, bias); }
    
    protected override void Awake()
    {
        onArrivedAtDestination += OnArrivedAtDestination;
        onArrivedAtNavMeshPosition += OnArrivedAtNavMeshPosition;

        FindAllPotentialHostileTargets(); // Finds all potential hostile targets in scene and adds it to the array
        FindMyPatrolPoints();

        base.Awake();
    }

    public void FindAllPotentialHostileTargets()
    {
        potentialHostileTargets.Clear();
        //Add only players to potential hostile targets if my enemy is player only
        if (hostileTargets == HostileTargets.Player)
        {
            AddAllPlayersInSceneToPotentialHostileTargets();
        }
        //Add only NPC characters to potential hostile targets if my enemy is NPC's
        else if(hostileTargets == HostileTargets.NPC)
        {
            AddAllNPCInSceneToPotentialHostileTargets();
        }
        //Add Both players and NPC characters to potential hostile targets if my enemy is both
        else if (hostileTargets == HostileTargets.Both)
        {
            AddAllPlayersInSceneToPotentialHostileTargets();
            AddAllNPCInSceneToPotentialHostileTargets();
        }
    }

    public void FindMyPatrolPoints()
    {
        //TODO, maybe have a level manager decide on the patrol points instead of this
        PatrolPoint[] p = FindObjectsOfType<PatrolPoint>() as PatrolPoint[];
        for(int i = 0; i <p.Length; i++)
        {
            myPatrolPoints.Add(p[i]);
        }
    }

    void AddAllPlayersInSceneToPotentialHostileTargets()
    {
        PlayerBrain[] p = FindObjectsOfType<PlayerBrain>() as PlayerBrain[];
        for (int i = 0; i < p.Length; i++)
        {
            potentialHostileTargets.Add(p[i].transform);
        }
    }

    void AddAllNPCInSceneToPotentialHostileTargets()
    {
        NPCBrain[] npc = FindObjectsOfType<NPCBrain>() as NPCBrain[];
        for (int i = 0; i < npc.Length; i++)
        {
            potentialHostileTargets.Add(npc[i].transform);
        }
    }


    protected override void Update()
    {
        if (_destination != null && _navMeshNextPosition != null)
        {
            // Is this the final destination, or just another position along the path?
            Vector3 nextPosition = _isNavMeshPositionFinal ? (Vector3)_destination : (Vector3)_navMeshNextPosition;

            // Check if at this position and fire off appropriate delegate
            if (CheckAtPosition(nextPosition, _destinationBias))
            {
                if (_isNavMeshPositionFinal)
                {
                    if (onArrivedAtDestination != null)
                        onArrivedAtDestination();
                }
                else
                {
                    if (onArrivedAtNavMeshPosition != null)
                        onArrivedAtNavMeshPosition();
                }
            }
            else
            {
                agent.SetDesiredVelocity(DirectionToPosition(nextPosition) * agent.properties.speed.max, true); //This is limiting to have here because AI speed can change depending on behaviour

                // Moodie: The below is not needed anymore since the rotation is handled automatically in agent now.

                //agent.RotateToVelocityDirection(agent.properties.speed.max);
                // hacky & temp :)
               // agent.currentRotation = Quaternion.LookRotation(DirectionToPosition(nextPosition));

                //agent.SetDesiredRotation(DirectionToPosition(nextPosition));
            }
            
        }
        base.Update();
    }

    //Patrol Logic
    public void Patrol()
    {
        //If patrol point exists set it as the destination
        if (_currentPatrolPoint)
            destination = _currentPatrolPoint.transform.position;
        //If patrol point does not exist grab one
        else
        {
            _currentPatrolPoint = GrabPatrolPoint();

            //If no patrol point was found set your destination to your position otherwise set it to the patrol point
            if (!_currentPatrolPoint)
                destination = transform.position;
            else
                destination = _currentPatrolPoint.transform.position;
        }

        print(_isArrivedAtDestination);
    }

    List<PatrolPoint> _nonBusyPoints = new List<PatrolPoint>(); // All patrol points that are not occupied by AI
    PatrolPoint _currentPatrolPoint; // My current patrol point
    public PatrolPoint GrabPatrolPoint()
    {
        _nonBusyPoints.Clear(); //Clear all non busy patrol points

        //If no patrol point exist in scene simply return null
        if (myPatrolPoints.Count == 0)
            return null;

        //Grab all patrol points that are not busy and place them in the nonbusy patrol points
        for (int i = 0; i < myPatrolPoints.Count; i++)
        {
            if (!myPatrolPoints[i].isBusy)
                _nonBusyPoints.Add(myPatrolPoints[i]);
        }

        //If no nonbusy patrol points were found simply return my currentPatrolPoint or null if does not exist
        if (_nonBusyPoints.Count == 0)
        {
            if (_currentPatrolPoint)
                return _currentPatrolPoint;
            else
                return null;
        }
        //Otherwise pick a random patrol point and set the last patrol point status to false
        else
        {
            if (_currentPatrolPoint)
                _currentPatrolPoint.SetStatus(false);

            int randomIndex = Random.Range(0, _nonBusyPoints.Count);

            _currentPatrolPoint = _nonBusyPoints[randomIndex];
            _currentPatrolPoint.SetStatus(true);
        }

        return _currentPatrolPoint;
    }

    //Clears the current patrol point by reseting status and nullifying it
    public void ClearPatrolPoint()
    {
        if (_currentPatrolPoint)
        {
            _currentPatrolPoint.SetStatus(false);
            _currentPatrolPoint = null;
        }    
    }

    //Chase hostile target logic
    public void ChaseHostileTarget()
    {
       
    }

    //------------------------------------------------------------------------------------------------------------------//
    // Cool Gizmo stuff below

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            if (_destination != null)
            {
                Gizmos.DrawLine(transform.position, (Vector3)_destination);
                Helpers.GizmoDrawRing((Vector3)_destination, _destinationBias);
            }
            if(_navMeshNextPosition != null)
                Gizmos.DrawLine(transform.position, (Vector3)_navMeshNextPosition);

            if (_navMeshPathToDestination != null)
            {
                NavMeshPath p = _navMeshPathToDestination;
                Gizmos.color = Color.red;
                for (int i = 0; i < p.corners.Length; i++)
                {
                    if ((i + 1) < p.corners.Length)
                        Gizmos.DrawLine(p.corners[i], p.corners[i + 1]);
                    Gizmos.DrawWireSphere(p.corners[i], 0.25f);
                }
            }
        }

        if(ShowGizmoz)
        {
            Transform t = this.transform;
            Handles.color = new Color(1, 0, 0, 0.1f);
            Handles.DrawSolidDisc(t.position, t.up, alertDistanceNear);


            float f = alertDistanceFarCone / 360f;
            Vector3 v = Vector3.Slerp(t.forward, -t.forward, f);

            Handles.DrawSolidArc(t.position,
                t.up,
                v,
                alertDistanceFarCone,
                alertDistanceFar);

            Handles.color = Color.yellow;
            Handles.DrawWireArc(t.position,
                t.up,
                v,
                alertDistanceFarCone,
                alertDistanceFar);


            //Handles.DrawSolidDisc(t.transform.position + v, Vector3.up, .5f);
            v *= alertDistanceFar;
            Handles.DrawLine(t.position + v, t.position);
            Handles.DrawLine(t.position + Vector3.Reflect(v, t.right), t.position);
        }
    }
}
