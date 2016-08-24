using UnityEngine;
using System.Collections;
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
	/// <summary>
	/// Ranking of an NPC to determine logic in terms of
	/// following and attack reactions
	/// </summary>
	public enum Rank
	{
		Boss = 0,
		WarLord = 1,
		Tank = 2,
		Soldier = 3,
		Weakling = 4,
		Nothing = 5
	}


	/// <summary>
	/// If the NPC follows superiors during patrol, this 
	/// will help guide how they do this 
	/// </summary>
	public enum PatrolFormation
	{
		Vee,		// A 'V' shaped formation behind a superior
		Column,		// Follow behind the superior in a straigh column
		Point,		// Take point, lead ahead of the superior
		Trail		// A lazy follow that trails behind 
	}




	[Tooltip("Contextual target that the NPC will use to base certain logic off (retreat, attack.. etc)")]
	public Transform target;
	public NPCProfile profile;

	public NPCBrain patrolSuperior = null;








	Vector3 _spawnSpot;
	public bool isSpawning { get { return agent.animationController.animatedGameObject.IsPlaying(agent.animationController.animationProperties.reaction.spawn.clip.name); } }

	protected override void Awake()
	{
		onArrivedAtDestination += OnArrivedAtDestination;
		onArrivedAtNavMeshPosition += OnArrivedAtNavMeshPosition;
		_attackController = GetComponent<AttackController>();
		_attackEnteredPosition = transform.position;
		SetAllAttacksAsArray();

		Debug.Assert(_attackController != null, "No Attack Controller");

		_spawnSpot = transform.position;

		base.Awake();
	}


	protected override void Start()
	{
		StartCoroutine(Spawn());
		_desiredMoveSpeed = agent.properties.speed.max;
		_attackController.SetOwnerHealthToDamageVolumes(agent.health);
		agent.health.SetStartingAndMax(profile.statistics.startingHealth, profile.statistics.maxHealth);
		_assignedPatrolZone = PatrolManager.GetNearestPatrolZone(this.transform);
		base.Start();
	}

	IEnumerator Spawn()
	{
		agent.animationController.Play(agent.animationController.animationProperties.reaction.spawn);
		while (isSpawning)
		{
			agent.isAllowedMovement = false;
			agent.isAllowedRotation = false;
			yield return null;
		}

		agent.isAllowedMovement = true;
		agent.isAllowedRotation = true;
		OnTransitionToNextState(State.Idle, State.Idle);
	}







	// -----------------------------------------------------------------------------------------------------------------
	// Attacks
	protected AttackController _attackController;
	private AttackDescriptor[] _attackCollectionArray = null;
	protected AttackDescriptor[] attackCollectionArray { get { return _attackCollectionArray; } }

	private void SetAllAttacksAsArray()
	{
		Debug.Assert(_attackCollectionArray == null, "Collection already full yet trying to fill again", this);
		_attackCollectionArray = new AttackDescriptor[9];
		_attackCollectionArray[0] = profile.attackCollection.melee1;
		_attackCollectionArray[1] = profile.attackCollection.melee2;
		_attackCollectionArray[2] = profile.attackCollection.melee3;
		_attackCollectionArray[3] = profile.attackCollection.melee4;
		_attackCollectionArray[4] = profile.attackCollection.melee5;
		_attackCollectionArray[5] = profile.attackCollection.melee6;
		_attackCollectionArray[6] = profile.attackCollection.ranged1;
		_attackCollectionArray[7] = profile.attackCollection.ranged2;
		_attackCollectionArray[8] = profile.attackCollection.special;
	}


	protected Vector3 _attackEnteredPosition;
	/// <summary>
	/// Position at which the NPC entered attack State
	/// </summary>
	public Vector3 attackEnteredPosition { get { return _attackEnteredPosition; } }


	// -----------------------------------------------------------------------------------------------------------------



	// Callbacks for the Brain arriving at positions of interest
	public delegate void ArrivedAtDestination();
	public event ArrivedAtDestination onArrivedAtDestination;
	public event ArrivedAtDestination onArrivedAtNavMeshPosition;


	// NPC's should modify these properties which this
	// base class will use to call the Agent
	protected Transform _desiredLookAtTarget = null;
	protected Vector3? _desiredLookAt = null;
	protected Vector3 _desiredMoveDirection = Vector3.zero;
	protected float _desiredMoveSpeed;

	protected virtual void OnArrivedAtDestination()
	{
		_navMeshNextPosition = null;
		_isNavMeshPositionFinal = false;
		_isArrivedAtDestination = false;
//		if (_patrolPoint != null)
	//		_patrolPoint.Release();
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



	protected Vector3 _desiredVelocity = Vector3.zero;

	// -----------------------------------------------------------------------------------------------------------------
	// Patrol 

	// Current patrol point the NPC is trying to reach
	public PatrolPoint _patrolPoint = null;

	// The zone this NPC gets assigned to upon spawning, it will be the nearest
	// on for now.  This is to limit 'wandering' of the NPC 
	public PatrolZone _assignedPatrolZone = null;


	private void PrepareNextPatrol()
	{
		if (_patrolPoint != null)
			_patrolPoint.Release();

		_patrolPoint = null;
		if (_assignedPatrolZone != null)
		{
			if (profile.patrol.stayWithinAssignedZone.checkProbability() || !_assignedPatrolZone.hasConnectedZones)
				_patrolPoint = _assignedPatrolZone.GetAvailablePatrolPoint();
			else
				_patrolPoint = _assignedPatrolZone.GetRandomConnectedZone().GetAvailablePatrolPoint();
		}

		if (_patrolPoint != null)
		{
			_patrolPoint.Occupy(this);
			destination = _patrolPoint.position;
		}
		else
			destination = transform.position;
	}



	// -----------------------------------------------------------------------------------------------------------------


	// -----------------------------------------------------------------------------------------------------------------
	//Personal space stuff here
	List<NPCBrain> NPCsInPersonalSpace = new List<NPCBrain>();
	List<NPCBrain> NPCsBlockingPath = new List<NPCBrain>();
	const float tangentDist = 1.0f; // This is a fixed value that represents a point tangentDist away from target destination 
									//in the direction perpendicular to direction of NPC and destination this is used to calculate the angle to check for enemies. As the NPC is closer to  target destination the bigger the angle
	const float minAngle = 10; //Angle to check cannot be less than this value
	const float maxAngle = 45;
	const float pathClearFactor = 1.15f; // Until path is clear by 1.5 times than the original angle keep avoiding NPC
	bool isAvoidingNPC = false;
	bool disableAvoidance = true;
	// -----------------------------------------------------------------------------------------------------------------






	// -----------------------------------------------------------------------------------------------------------------
	// Nav Mesh and General Pathfinding 

	public Vector3 directionToDestination { get { return ((Vector3)_destination - transform.position).normalized; } }
	public Vector3 DirectionToPosition(Vector3 position) { return (position - transform.position).normalized; }
	public bool CheckAtPosition(Vector3 position) { return CheckAtPosition(position, 0.01f); }
	public bool CheckAtPosition(Vector3 position, float bias) { return Helpers.InRadiusGrounded(transform.position, position, bias); }

	
	private NavMeshPath _navMeshPathToDestination;

	protected Vector3? _navMeshNextPosition;
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
			if (CheckAtPosition((Vector3)value, profile.patrol.destinationBias))
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

	// -----------------------------------------------------------------------------------------------------------------










	#region State Updates and Management


	// -----------------------------------------------------------------------------------------------------------------
	// State Routines/Management


	// Holds pointer to the current state logic to run
	private IEnumerator _stateLogicRoutine;
	private IEnumerator _internalStateLogicRoutine;

	protected virtual IEnumerator Update_PatrolState() { yield return null; }
	protected virtual IEnumerator Update_RetreatState() { yield return null; }
	protected virtual IEnumerator Update_AttackState() { yield return null; }
	protected virtual IEnumerator Update_IdleState() { yield return null; }

	protected bool isIgnoreInternalUpdateRoutine = false;

	private IEnumerator InternalUpdate_PatrolState()
	{
		if (!isIgnoreInternalUpdateRoutine)
		{
			_desiredMoveSpeed = agent.properties.speed.max * profile.patrol.patrolMoveSpeedRatio;
			while (true)
			{
				patrolSuperior = null;

				PrepareNextPatrol();


				// Follow path until finished
				while (_navMeshNextPosition != null && patrolSuperior == null)
				{
					if (profile.patrol.followSuperiors.checkProbability())
					{
						// TODO - optimise with Layer masks
						Collider[] c = Physics.OverlapSphere(transform.position, 15f);
						for (int i = 0; i < c.Length; i++)
						{
							NPCBrain b = c[i].GetComponent<NPCBrain>();
							if (b != null)
							{
								if (b != this && !b.agent.health.isDead)
								{
									if ((int)profile.statistics.rank > (int)b.profile.statistics.rank)
									{
										patrolSuperior = b;
										break;
									}
								}
							}
						}
					}
					yield return new WaitForSeconds(2f);
				}

				if (_patrolPoint != null)
					_patrolPoint.Release();

				// No path, then maybe superior to follow?
				while(patrolSuperior != null)
				{
					//(patrolSuperior.transform.forward * profile.awareness.personalSpace) + 
					Vector3 distanceBehind = (patrolSuperior.transform.forward * patrolSuperior.profile.awareness.personalSpace);
					Vector3 newDest = patrolSuperior.transform.position - distanceBehind;
					destination = newDest;

					if (Helpers.InRadius(newDest, transform, profile.attack.closeRangeDistance * 0.5f))
						_desiredMoveSpeed = agent.properties.speed.max * profile.patrol.patrolMoveSpeedRatio;
					else
						_desiredMoveSpeed = agent.properties.speed.max;

					yield return new WaitForSeconds(Random.value * 2f);					
				}


				// If probability to hold position
				// Then wait at this position
				if (profile.patrol.waitAtDestination.checkProbability())
					yield return new WaitForSeconds(profile.patrol.waitAtDestinationTime);

				yield return null;
			}
		}
	}

	private IEnumerator InternalUpdate_RetreatState()
	{
		if (!isIgnoreInternalUpdateRoutine)
		{
			destination = _spawnSpot;
			yield return null;
		}
	}


	public bool _isInCloseRangeRoutine = false;
	public bool _isAllowedCloseRangeRoutine = true;
	IEnumerator ActivateInCloseRangeForTime(float time)
	{
		_isInCloseRangeRoutine = true;
		_isAllowedCloseRangeRoutine = false;
		yield return new WaitForSeconds(time);
		print("finished routine...  now wait before allowed again");
		_isInCloseRangeRoutine = false;
		yield return new WaitForSeconds(time);
		print("allowed again!");
		_isAllowedCloseRangeRoutine = true;
	}

	private IEnumerator InternalUpdate_AttackState()
	{
		if (!isIgnoreInternalUpdateRoutine)
		{
			// Upon entering attack play an 'alert' animation and look at the target
			agent.animationController.Play(profile.actions.roar);
			_desiredMoveSpeed = 0f;
			while (agent.animationController.animatedGameObject.IsPlaying(profile.actions.roar.clip.name))
			{
				agent.SetDesiredRotation(target);
				yield return null;
			}
			_desiredMoveSpeed = agent.properties.speed.max;
			_desiredLookAtTarget = target;


			while (target != null)
			{
				ClearQueuedAction();

				if (_attackController.isPastYieldControlTime)
				{
					List<AttackDescriptor> aDescs;
					if (!_isInCloseRangeRoutine)
					{
						if (profile.attack.preferCloseCombat.checkProbability() && _isAllowedCloseRangeRoutine)
						{
							StartCoroutine(ActivateInCloseRangeForTime(profile.attack.closeCombatDuration));
							aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollectionArray, target, profile.attack.closeRangeDistance);
						}
						else
							aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollectionArray, target);
					}
					else
						aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollectionArray, target, profile.attack.closeRangeDistance);

					if (aDescs != null)
					{						
						AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];										
						_attackController.AttackWithDescriptor(ad, target);
					}
					else					
						destination = target.position;					
				}

				// If within close range, move at close range speed
				// If outside of far range, move at max speed
				if (Helpers.InRadius(transform.position, target, profile.attack.closeRangeDistance))
				{
					_desiredMoveSpeed = agent.properties.speed.max * profile.attack.closeRangeMoveSpeedRatio;
					_desiredLookAtTarget = target;
				}
				if (!Helpers.InRadius(transform.position, target, profile.attack.farRangeDistance))
				{
					_desiredMoveSpeed = agent.properties.speed.max;
					_desiredLookAtTarget = null;
					_desiredLookAt = null;
					destination = target.position;
				}


				// Check if too far away from the target or too
				// far away from where we started this attack state
				if ((!Helpers.InRadius(transform, target, profile.attack.stopAttackFromTargetDistance))
						|| (!Helpers.InRadius(transform, _attackEnteredPosition, profile.attack.stopAttackFromStartDistance)))
					state = State.Idle;

				yield return null;
			}

			// If we got here then the target is null, so 
			// there's nothing more to attack 
			state = State.Idle;
		}
	}

	private IEnumerator InternalUpdate_IdleState()
	{
		if (!isIgnoreInternalUpdateRoutine)
		{
			if (_previousState == State.Attack)
			{
				destination = _attackEnteredPosition;
				_desiredMoveSpeed = agent.properties.speed.max * profile.patrol.patrolMoveSpeedRatio;

				while (_navMeshNextPosition != null)
					yield return null;
			}
			else
				destination = transform.position;

			if (profile.patrol.waitAtDestination.checkProbability())
				yield return new WaitForSeconds(profile.patrol.waitAtDestinationTime);

			state = State.Patrol;

			yield return null;
		}
	}




	/// <summary>
	/// Transitions to the next state by assigning the appropriate update routine.
	/// Override this method to add transition logic on an NPC
	/// </summary>	
	protected virtual void OnTransitionToNextState(State from, State to)
	{
		if (to == State.Attack)
		{
			if (from == State.Patrol)
				_attackEnteredPosition = _navMeshNextPosition != null ? (Vector3)_navMeshNextPosition : transform.position;
			if (from == State.Idle)
				_attackEnteredPosition = transform.position;
		}
			

		// Cancel the currently running coroutine
		if (_stateLogicRoutine != null)
		{
			StopCoroutine(_stateLogicRoutine);
			StopCoroutine(_internalStateLogicRoutine);
		}

		isIgnoreInternalUpdateRoutine = false;
		if (_patrolPoint != null)
			_patrolPoint.Release();

		// Set the next co-routine and perform basic 
		// logic related to each state transition
		switch (to)
		{
			//  Idle -----------------------------------------------------
			case State.Idle:
				_stateLogicRoutine = Update_IdleState();
				_internalStateLogicRoutine = InternalUpdate_IdleState();				
				break;

			//  Patrol -----------------------------------------------------
			case State.Patrol:
				_stateLogicRoutine = Update_PatrolState();
				_internalStateLogicRoutine = InternalUpdate_PatrolState();
				break;

			//  Attack -----------------------------------------------------
			case State.Attack:
				_stateLogicRoutine = Update_AttackState();
				_internalStateLogicRoutine = InternalUpdate_AttackState();
				break;

			//  Retreat -----------------------------------------------------
			case State.Retreat:
				_stateLogicRoutine = Update_RetreatState();
				_internalStateLogicRoutine = InternalUpdate_RetreatState();				
				break;
		}
		StartCoroutine(_stateLogicRoutine);
		StartCoroutine(_internalStateLogicRoutine);
	}

	/// <summary>
	/// Logic state of the NPC
	/// </summary>
	public enum State
	{
		Idle = 0,
		Patrol = 1,
		Retreat = 2,
		Attack = 3
	}

	protected State _previousState;
	public State previousState { get { return _previousState; } }
	private State _state = State.Idle;
	public State state
	{
		get { return _state; }
		set
		{
			if (_state == value)
				return;
			else
			{
				_previousState = _state;
				_state = value;
				OnTransitionToNextState(_previousState, _state);
			}
		}
	}

	// -----------------------------------------------------------------------------------------------------------------
	// -----------------------------------------------------------------------------------------------------------------



	#endregion




	// -----------------------------------------------------------------------------------------------------------------
	public bool IsTargetWithinAwareness(Transform targetTransform)
	{
		if (target == null) return false;
		return Helpers.InConeAngleAndRadius(targetTransform, transform, profile.awareness.coneAngle, profile.awareness.coneRadius)
			|| Helpers.InRadius(transform, targetTransform, profile.awareness.radial);
	}
	// -----------------------------------------------------------------------------------------------------------------	


	
	private bool _isActionQueued = false;
	private AttackDescriptor _queuedAction = null;	
	private void ClearQueuedAction()
	{
		if (_isActionQueued && _queuedAction != null)
		{
			_attackController.YieldControlFromAttack(true);
			_attackController.AttackWithDescriptor(_queuedAction);
			_isActionQueued = false;
		}
	}




	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if (info.responsibleAttackController == null)
			return;

		if (info.responsibleGameObject.transform == target)			
		{
			state = State.Attack;
			if (profile.attack.jumpAwayIfHit.checkProbability())
			{
				if (_attackController.YieldControlFromAttack())
				{
					agent.SetDesiredRotation(Helpers.DirectionTo(transform, target), true);
					_attackController.AttackWithDescriptor(profile.actions.jumpAway);
				}
				else
				{
					_isActionQueued = true;
					_queuedAction = profile.actions.jumpAway;
				}
			}
			return;
		}

		if (info.responsibleAttackController.GetComponent<ExplorerBrain>())
		{
			state = State.Attack;
			target = info.responsibleAttackController.transform;
		}

		base.OnHealthLost(info);
	}





	protected override void Update()
	{

		// If staggered don't bother movement update
		if (agent.isStaggered || isSpawning)
			return;
		/*
		if (!disableAvoidance)
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
		*/

		// Check if this NPC wants to move to a preferred state on aware
		if (profile.awareness.isUsePreferredStateOnAware && state != profile.awareness.preferredStateOnAware)
			if (IsTargetWithinAwareness(target))
				state = profile.awareness.preferredStateOnAware;

		// Continue to pathfind
		if (_destination != null && _navMeshNextPosition != null)
		{
			Vector3 nextPosition = (Vector3)_navMeshNextPosition;

			// Check if at this position and fire off appropriate delegate
			if (CheckAtPosition(nextPosition, profile.patrol.destinationBias))
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


	protected bool isLookAtPlayer = false;
	protected void MoveAgent()
	{
		if (!_attackController.isControllingAgentVelocity && !isLookAtPlayer)
		{
			if (_desiredLookAtTarget != null)
				agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, _desiredLookAtTarget.position);
			else if (_desiredLookAt != null)
				agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, (Vector3)_desiredLookAt);
			else
				agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, true);
		}
		else if (!_attackController.isControllingAgentVelocity && isLookAtPlayer && LevelManager.players[0])
			agent.SetDesiredVelocity(_desiredMoveDirection * _desiredMoveSpeed, LevelManager.players[0].transform.position);
	}






	#region blocking
	// -----------------------------------------------------------------------------------------------------------------
	// NPC Path Blocking 
	void UpdateNPCsInPersonalSpace()
	{
		NPCsInPersonalSpace.Clear(); // Clear all NPCs in the personal space
		Collider[] c = Physics.OverlapSphere(transform.position, profile.awareness.personalSpace); //Grab all colliders in the personal space
		for (int i = 0; i < c.Length; i++)
		{
			NPCBrain b = c[i].transform.GetComponent<NPCBrain>(); //If collider is an NPCBrain add to the NPC in personal space list
			if (b && b != this)
			{
				NPCsInPersonalSpace.Add(b);
			}
		}
	}

	bool IsNPCsBlockingPath()
	{
		if (_navMeshNextPosition == null) //If I am not moving there is no reason to check if any NPC is blocking my path
			return false;
		else
		{
			NPCsBlockingPath.Clear();
			Vector3 dir = Helpers.DirectionTo(transform.position, (Vector3)_navMeshNextPosition); // Grab a direction to my next direction
			Vector3 dest = (Vector3)_navMeshNextPosition; //Store my next destination as a vector
			Vector3 currentPos = transform.position; //Store my current destination as a vector
			dest.y = 0; // Zero out Y
			currentPos.y = 0; // Zero out Y
			float distaneToTarget = Vector3.Distance(currentPos, dest); // Calculate distance from my position to my destination
			float angle = Mathf.Atan(tangentDist / distaneToTarget) * Mathf.Rad2Deg * 2; //Calculate angle needed to use if an NPC is blocking my path. The closer I am to my destination the bigger the angle
																						 // TODO IS THE ABOVE IMPORTANT? MAYBE JUST FIX ANGLE IS FINE....
			angle = Mathf.Clamp(angle, minAngle, maxAngle); //Clamp angle between a min and max
			float fact = 1;
			if (isAvoidingNPC) // If my current status is avoiding an NPC, multiplying the angle by a factor to avoid stuttering
				fact = pathClearFactor;

			//For all NPCs in personal space check if they are blocking my path in a cone infront of me given the angle
			for (int i = 0; i < NPCsInPersonalSpace.Count; i++)
			{
				Vector3 npcPos = NPCsInPersonalSpace[i].transform.position;
				npcPos.y = 0;
				if (MathLab.IsTargetInCone(currentPos, npcPos, angle * fact, dir, profile.awareness.personalSpace))
				{
					NPCsBlockingPath.Add(NPCsInPersonalSpace[i]);
				}
			}
		}
		//If count is zero, then no NPC is blocking my path
		if (NPCsBlockingPath.Count == 0)
			return false;
		else
			return true;
	}

	Vector3 GrabMoveDirectionAwayFromBlockedPath()
	{
		//From all NPCs that are blocking my path calculate the average position
		Vector3 averagePos = Vector3.zero;
		Vector3 tempPos = Vector3.zero;
		for (int i = 0; i < NPCsBlockingPath.Count; i++)
		{
			tempPos = NPCsBlockingPath[i].transform.position;
			tempPos.y = 0;
			averagePos += tempPos;
		}
		averagePos = averagePos / NPCsBlockingPath.Count;

		Vector3 dir1 = Helpers.DirectionTo(transform.position, averagePos); // Grab a direction to the average position of NPCs blocking my path
		Vector3 dir2 = Helpers.DirectionTo(transform.position, (Vector3)_navMeshNextPosition); // Grab a direction to my destination
		if (Vector3.Dot(dir1, dir2) <= 0) // If dot product is negative NPC on my left, other wise on my right
			return Vector3.Cross(dir1, Vector3.down);
		else
			return Vector3.Cross(dir1, Vector3.up);
	}

	protected Vector3 FindRetreatTarget()
	{
		//TODO Have some logic here to find retreat points set up by player or dynamic like hide behind a grunt or something.

		//If no specific retreat point is found simply run away from target in the opposite direction
		Vector3 dest = transform.position;
		Vector3 playerPos;
		if (LevelManager.players[0])
			playerPos = LevelManager.players[0].transform.position; //TODO, if multiplayer mode need to change this
		else
			return dest;

		dest += Helpers.DirectionTo(playerPos, transform.position) * 15; //Destination is X units in a direction away from the player

		if (Helpers.IsPointOutsideNavMesh(dest))
		{
			NavMeshHit hit;
			if (NavMesh.SamplePosition(dest, out hit, 30, NavMesh.AllAreas))
			{
				dest = hit.position;
			}
		}

		return dest;
	}
	// -----------------------------------------------------------------------------------------------------------------
	#endregion




	void OnDrawGizmos()
	{
		if (!debugDraw.isActive)
			return;

		if (Application.isPlaying)
		{
			if (debugDraw.DRAW_PATROL)
			{
				Gizmos.color = Color.yellow;
				if (_destination != null)
				{
					Helpers.Draw.GizmoRing((Vector3)_destination, profile.patrol.destinationBias);
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
						Helpers.Draw.GizmoRing(p.corners[i], profile.patrol.destinationBias);
					}
				}
			}			
		}
	}


}