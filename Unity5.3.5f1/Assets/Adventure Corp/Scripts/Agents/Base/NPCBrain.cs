using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor;


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

	public enum State
	{
		Idle,
		Patrol,
		Retreat,
		Attack
	}

	private State _state = State.Idle;
	public State state
	{
		get { return _state; }
		set { _state = value; }
	}


    public bool isDebugOn = false;
	// Callbacks for the Brain arriving at positions of interest
	public delegate void ArrivedAtDestination();
	public event ArrivedAtDestination onArrivedAtDestination;
	public event ArrivedAtDestination onArrivedAtNavMeshPosition;

	public BaseAttackCollection attackCollection;
	protected AttackController _attackController;   



	// NPC's should modify these properties which this
	// base class will use to call the Agent
	protected Vector3? _desiredLookAt = null;
	protected Vector3 _desiredMoveDirection = Vector3.zero;
	protected float _desiredMoveSpeed;



	protected virtual void OnArrivedAtDestination()
	{
		_navMeshNextPosition = null;
		_isNavMeshPositionFinal = false;
		_isArrivedAtDestination = false;
	}


	protected virtual void OnArrivedAtNavMeshPosition()
	{
		if (_navMeshNextPositionIndex < _navMeshPathToDestination.corners.Length - 1)
		{
			_navMeshNextPositionIndex++;

			if (_navMeshNextPositionIndex == _navMeshPathToDestination.corners.Length - 1)
				_isNavMeshPositionFinal = true;

			_navMeshNextPosition = _navMeshPathToDestination.corners[_navMeshNextPositionIndex];
		}
		Debug.Assert(_navMeshNextPositionIndex <= _navMeshPathToDestination.corners.Length);
	}


	// TODO - this should go into a properties container?
	private float _destinationBias = 1;

	protected Vector3 _desiredVelocity = Vector3.zero;

    //-------------------------------------------------------//
    //Patrol stuff here
    [HideInInspector]
    public PatrolProperties patrolProperties;    
    //-------------------------------------------------------//

    //-------------------------------------------------------//
    //Personal space stuff here
    List<NPCBrain> NPCsInPersonalSpace = new List<NPCBrain>();
    List<NPCBrain> NPCsBlockingPath = new List<NPCBrain>();
    float personalSpaceRadius = 1.0f;
    const float tangentDist = 1.0f; // This is a fixed value that represents a point tangentDist away from target destination 
    //in the direction perpendicular to direction of NPC and destination this is used to calculate the angle to check for enemies. As the NPC is closer to  target destination the bigger the angle
    const float minAngle = 10; //Angle to check cannot be less than this value
    const float maxAngle = 45;
    const float pathClearFactor = 1.15f; // Until path is clear by 1.5 times than the original angle keep avoiding NPC
    bool isAvoidingNPC = false;
    public bool disableAvoidance = false;
    //-------------------------------------------------------//


    private NavMeshPath _navMeshPathToDestination;

	private Vector3? _navMeshNextPosition;
	private int _navMeshNextPositionIndex = 0;
	public int navMeshNextPositionIndex { get { return _navMeshNextPositionIndex; } }
	private bool _isNavMeshPositionFinal = false;

	protected bool _isArrivedAtDestination = false;
	private Vector3? _destination = null;
	public Vector3? destination
	{
		set
		{
			// If already there, exit
			if (CheckAtPosition((Vector3)value, _destinationBias))
			{
				_destination = null;
				return;
			}

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
		// Clear up current path
		_navMeshNextPosition = null;
		_isNavMeshPositionFinal = false;
		_isArrivedAtDestination = false;

		_navMeshPathToDestination = new NavMeshPath();
		if (NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, _navMeshPathToDestination))
		{
			_navMeshNextPositionIndex = 1;
			_navMeshNextPosition = _navMeshPathToDestination.corners[_navMeshNextPositionIndex];
			_destination = _navMeshPathToDestination.corners[_navMeshPathToDestination.corners.Length - 1];

			if (_navMeshPathToDestination.corners.Length <= 2)
				_isNavMeshPositionFinal = true;

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
	public bool CheckAtPosition(Vector3 position, float bias) { return Helpers.InRadiusGrounded(transform.position, position, bias); }

	public bool isSpawning { get { return agent.animationController.animatedGameObject.IsPlaying(agent.animationController.animationProperties.reaction.spawn.clip.name); } }

	protected override void Awake()
	{
		onArrivedAtDestination += OnArrivedAtDestination;
		onArrivedAtNavMeshPosition += OnArrivedAtNavMeshPosition;		
		_attackController = GetComponent<AttackController>();
		Debug.Assert(_attackController != null, "No Attack Controller");
		base.Awake();
	}

	IEnumerator Spawn()
	{
		agent.animationController.Play(agent.animationController.animationProperties.reaction.spawn);
		while(isSpawning)
		{
			agent.isAllowedMovement = false;
			agent.isAllowedRotation= false;
			yield return null;
		}

		agent.isAllowedMovement = true;
		agent.isAllowedRotation = true;
	}

	protected override void Start()
	{
		StartCoroutine(Spawn());
		_desiredMoveSpeed = agent.properties.speed.max;
		_attackController.SetOwnerHealthToDamageVolumes(agent.health);
		StartCoroutine(LogicRoutineInternal());
        StartCoroutine(Patrol());
        personalSpaceRadius = agent.controller.radius * 3.5f;//Personal space radius is always three times the size of the radius of the controller
        base.Start();
	}


	protected abstract IEnumerator LogicRoutine();
	private IEnumerator LogicRoutineInternal()
	{
		while (isSpawning)
            yield return null;

		while (!agent.health.isDead)
		{
			yield return LogicRoutine();
			yield return null;
		}
	}
	

    private IEnumerator Patrol()
    {
        while (isSpawning)
           yield return null;

        while (!agent.health.isDead)
        {
            if (state != State.Patrol)
                yield return null;
            else
            {
                if (PatrolManager.instance)
                    patrolProperties = PatrolManager.instance.GrabPatrolProperties(this);

                if (!patrolProperties.patrolPoint)
                    destination = transform.position;
                else
                    destination = patrolProperties.patrolPoint.transform.position;

                while (_navMeshNextPosition != null)
                    yield return null;

                yield return new WaitForSeconds(2.5f);

                yield return null;
            }
        }
    }


    
    protected override void Update()
    {
        // If staggered don't bother movement update
        if (agent.isStaggered || isSpawning)
            return;

        if(!disableAvoidance)
        {
            UpdateNPCsInPersonalSpace(); // Add all NPCs in your personal space
            if (NPCsInPersonalSpace.Count > 0) // Check if there is any NPCs in your personal space
            {
                if (IsNPCsBlockingPath()) // Check if any NPCs in personal space blocking you path
                {
                    isAvoidingNPC = true;
                    Vector3 moveDir = GrabMoveDirectionAwayFromBlockedPath(); // Get a movement direction to avoid bumping into NPC
                    _desiredMoveDirection = moveDir;
                    MoveAgent();
                    return;
                }
            }

            isAvoidingNPC = false;
        }
        
        if (_destination != null && _navMeshNextPosition != null)
		{
			Vector3 nextPosition = (Vector3)_navMeshNextPosition;

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
				_desiredMoveDirection = DirectionToPosition(nextPosition);
				MoveAgent();
			}
		}
		base.Update();
	}


	protected void MoveAgent()
	{
		if (!_attackController.isControllingAgentVelocity)
		{
			if(_desiredLookAt != null)
				agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, (Vector3)_desiredLookAt);
			else
				agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, true);
		}
	}

    void UpdateNPCsInPersonalSpace()
    {
        NPCsInPersonalSpace.Clear();
        Collider[] c = Physics.OverlapSphere(transform.position, personalSpaceRadius);
        for(int i = 0; i < c.Length; i++)
        {
            NPCBrain b = c[i].transform.GetComponent<NPCBrain>();
            if(b && b != this)
            {
                NPCsInPersonalSpace.Add(b);
            }
        }
    }

    bool IsNPCsBlockingPath()
    {
        if (_navMeshNextPosition == null)
            return false;
        else
        {
            NPCsBlockingPath.Clear();			
            Vector3 dir = Helpers.DirectionTo(transform.position, (Vector3)_navMeshNextPosition);
            Vector3 dest = (Vector3)_navMeshNextPosition;
            Vector3 currentPos = transform.position;
            dest.y = 0;
            currentPos.y = 0;
            float distaneToTarget = Vector3.Distance(currentPos, dest);
            float angle = Mathf.Atan(tangentDist / distaneToTarget) * Mathf.Rad2Deg * 2;
            angle = Mathf.Clamp(angle, minAngle, maxAngle);
            float fact = 1;
            if (isAvoidingNPC)
                fact = pathClearFactor;
            for (int i = 0; i < NPCsInPersonalSpace.Count; i++)
            {
                Vector3 npcPos = NPCsInPersonalSpace[i].transform.position;
                npcPos.y = 0;
                if (MathLab.IsTargetInCone(currentPos, npcPos, angle * fact, dir, personalSpaceRadius))
                {
                    NPCsBlockingPath.Add(NPCsInPersonalSpace[i]);
                }
            }
        }
        if (NPCsBlockingPath.Count == 0)
            return false;
        else
            return true;
    }

    Vector3 GrabMoveDirectionAwayFromBlockedPath()
    {
        Vector3 averagePos = Vector3.zero;
        Vector3 tempPos = Vector3.zero;
        for(int i = 0; i < NPCsBlockingPath.Count; i ++)
        {
            tempPos = NPCsBlockingPath[i].transform.position;
            tempPos.y = 0;
            averagePos += tempPos;
        }
        averagePos = averagePos / NPCsBlockingPath.Count;
        Vector3 dir1 = Helpers.DirectionTo(transform.position, averagePos);
        Vector3 dir2 = Helpers.DirectionTo(transform.position, (Vector3)_navMeshNextPosition);
        if (Vector3.Dot(dir1, dir2) <= 0)
            return Vector3.Cross(dir1, Vector3.down);
        else
            return Vector3.Cross(dir1, Vector3.up);
    }

	void OnDrawGizmos()
	{
        if (!isDebugOn)
            return;

		if (Application.isPlaying)
		{
			Gizmos.color = Color.yellow;
			if (_destination != null)
			{
				//Gizmos.DrawLine(transform.position, (Vector3)_destination);
				Helpers.GizmoDrawRing((Vector3)_destination, _destinationBias);
			}

			Gizmos.color = Color.cyan;
			if (_navMeshNextPosition != null)
				Gizmos.DrawLine(transform.position + Vector3.up, (Vector3)_navMeshNextPosition);

			if (_navMeshPathToDestination != null)
			{
				NavMeshPath p = _navMeshPathToDestination;
				Gizmos.color = Color.red;
				for (int i = 0; i < p.corners.Length; i++)
				{
					if ((i + 1) < p.corners.Length)
						Gizmos.DrawLine(p.corners[i], p.corners[i + 1]);
					Helpers.GizmoDrawRing(p.corners[i], _destinationBias);
				}
			}

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, personalSpaceRadius); // Debug personal space sphere
		}
	}
}











//if(ShowGizmoz)
//{
//    Transform t = this.transform;
//    Handles.color = new Color(1, 0, 0, 0.1f);
//    Handles.DrawSolidDisc(t.position, t.up, alertDistanceNear);


//    float f = alertDistanceFarCone / 360f;
//    Vector3 v = Vector3.Slerp(t.forward, -t.forward, f);

//    Handles.DrawSolidArc(t.position,
//        t.up,
//        v,
//        alertDistanceFarCone,
//        alertDistanceFar);

//    Handles.color = Color.yellow;
//    Handles.DrawWireArc(t.position,
//        t.up,
//        v,
//        alertDistanceFarCone,
//        alertDistanceFar);


//    //Handles.DrawSolidDisc(t.transform.position + v, Vector3.up, .5f);
//    v *= alertDistanceFar;
//    Handles.DrawLine(t.position + v, t.position);
//    Handles.DrawLine(t.position + Vector3.Reflect(v, t.right), t.position);
//}




//public enum HostileTargets
//{
//    Player,
//    NPC,
//    Both
//}



//public AIProperties properties;

////---------------------------------------------------------------------------------------------------------------//
////Hostile targets variables and functions
//Transform hostileTarget; //Behviour of AI can change if hostileTarget is not null
//List<Transform> potentialHostileTargets = new List<Transform>(); //AI will look for hostile targets in this array

//public HostileTargets hostileTargets; //Determines which objects count as a hostile target (Player, NPC or both)

//// Alert details that are specific 
//// to the brain/character
//// Callbacks for the Brain arriving at positions of interest
//public delegate void HostileTargetSpotted();
//public event HostileTargetSpotted onHostileTargetSpotted; //This event fires off when an enemy spots a hostile target
//public delegate void HostileTargetCleared();
//public event HostileTargetCleared onHostileTargetCleared; //This event fires off when an enemy does not have a hostile target anymore

//public float alertDistanceFar = 5; // How far ahead can the agent detect hostile targets
//public float alertDistanceFarCone = 45; // In what angle can the angent spot infront of him
//public float alertDistanceNear = 1; // How close can the agent detect hostile targets without looking in the cone

////TODO: How to avoid NPC detecting player if he is behind walls or above him in the y direction 
//// TODO : (Matt)  ...use sqrt distance to optimise instead of Vec3.Distance()
//// ....already a Helper function for this 
//bool HostileTargetInFarRange(Transform t) { return (Vector3.Distance(t.position, transform.position) < alertDistanceFar); }
//bool HostileTargetInNearRange(Transform t) { return (Vector3.Distance(t.position, transform.position) < alertDistanceNear); }
//bool HostileTargetInFarConeAndDistance(Transform t) { return HostileTargetInFarRange(t) && HostileTargetInFarCone(t); }
//bool HostileTargetInFarCone(Transform t)
//{
//    Vector3 dirToTarget = (t.position - transform.position).normalized;
//    return (Vector3.Angle(dirToTarget, transform.forward) < alertDistanceFarCone * 0.5f);
//}

///// <summary>
///// Finds if a potential hostile target is in any range of the AI and sets it as the hostile target
///// </summary>
//public bool HostileTargetInAnyRange()
//{
//    if (potentialHostileTargets.Count == 0)
//        return false;

//    for (int i = 0; i < potentialHostileTargets.Count; i++)
//    {
//        if (HostileTargetInNearRange(potentialHostileTargets[i]) || HostileTargetInFarConeAndDistance(potentialHostileTargets[i]))
//        {
//            //TODO store all potential hostile targets in an array and then check which one is the closest one and pick that one as a hostile target
//            SetHostileTarget(potentialHostileTargets[i]);
//            return true;
//        }
//    }
//    return false;
//}


//public void SetHostileTarget(Transform t)
//{
//    hostileTarget = t;

//    if (onHostileTargetSpotted != null)
//        onHostileTargetSpotted();
//}

//public void RemoveHostileTarget()
//{
//    if (hostileTarget == null)
//        return;

//    hostileTarget = null;

//    if (onHostileTargetCleared != null)
//        onHostileTargetCleared();
//}


//public void FindAllPotentialHostileTargets()
//{
//    potentialHostileTargets.Clear();
//    //Add only players to potential hostile targets if my enemy is player only
//    if (hostileTargets == HostileTargets.Player)
//    {
//        AddAllPlayersInSceneToPotentialHostileTargets();
//    }
//    //Add only NPC characters to potential hostile targets if my enemy is NPC's
//    else if (hostileTargets == HostileTargets.NPC)
//    {
//        AddAllNPCInSceneToPotentialHostileTargets();
//    }
//    //Add Both players and NPC characters to potential hostile targets if my enemy is both
//    else if (hostileTargets == HostileTargets.Both)
//    {
//        AddAllPlayersInSceneToPotentialHostileTargets();
//        AddAllNPCInSceneToPotentialHostileTargets();
//    }
//}

//void AddAllPlayersInSceneToPotentialHostileTargets()
//{
//    for (int i = 0; i < LevelManager.players.Count; i++)
//    {
//        potentialHostileTargets.Add(LevelManager.players[i].transform);
//    }
//}

//void AddAllNPCInSceneToPotentialHostileTargets()
//{
//    for (int i = 0; i < LevelManager.players.Count; i++)
//    {
//        potentialHostileTargets.Add(LevelManager.players[i].transform);
//    }
//}

//public bool ShowGizmoz = true;
//List<PatrolPoint> myPatrolPoints = new List<PatrolPoint>();

////-------------------------------------------------------------------------------------------------------------------//


