using UnityEngine;
using System.Collections.Generic;

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
        base.Awake();
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
                agent.SetVelocity(DirectionToPosition(nextPosition) * agent.properties.speed.max);
            
        }
        base.Update();
    }

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
}
