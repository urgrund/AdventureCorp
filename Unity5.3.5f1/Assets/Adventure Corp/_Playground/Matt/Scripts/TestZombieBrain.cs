﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestZombieBrain : NPCBrain
{
	/*Vector3 _spawnSpot;

	protected override void Awake()
	{
		_spawnSpot = transform.position;
		base.Awake();
	}

	protected override void OnTransitionToNextState(State from, State to)
	{
		Debug.Log("Transitioned from " + from + "  to " + to);
		base.OnTransitionToNextState(from, to);
	}

	protected override IEnumerator UpdateIdleState()
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

		if(profile.patrol.waitAtDestination.checkProbability())
			yield return new WaitForSeconds(profile.patrol.waitAtDestinationTime);
		state = State.Patrol;
	}


	protected override IEnumerator UpdateRetreatState()
	{
		//print("In Retreat");
		destination = _spawnSpot;
		while (true)
		{			
			yield return new WaitForSeconds(0.5f);
		}
	}


	protected override IEnumerator UpdatePatrolState()
	{
		//print("In Patrol");
		_desiredMoveSpeed = agent.properties.speed.max * profile.patrol.patrolMoveSpeedRatio;
		while (true)
		{
			if (PatrolManager.instance)
				patrolProperties = PatrolManager.instance.GrabPatrolProperties(this); 

			if (!patrolProperties.patrolPoint) 
				destination = transform.position;
			else
				destination = patrolProperties.patrolPoint.transform.position; 

			while (_navMeshNextPosition != null) 
				yield return null;

			// If probability to hold position
			// Then wait at this position
			if(profile.patrol.waitAtDestination.checkProbability())
				yield return new WaitForSeconds(profile.patrol.waitAtDestinationTime);

			yield return null;
		}
	}

	public List<AttackDescriptor> aDescs;
	protected override IEnumerator UpdateAttackState()
	{
		//print("In Attack");
		while (target != null)
		{
			aDescs = _attackController.GetSuggestedAttacksForTarget(profile.attackCollection, target);
			if (aDescs != null)
			{
				print("found");
				AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];
				_attackController.AttackWithDescriptor(ad);
			}
			else
			{
				destination = target.position;
			}

			// If within close range, move at close range speed
			// If outside of far range, move at max speed
			if (Helpers.InRadius(transform.position, target, profile.attack.closeRangeDistance))
				_desiredMoveSpeed = agent.properties.speed.max * profile.attack.closeRangeMoveSpeedRatio;
			if (!Helpers.InRadius(transform.position, target, profile.attack.farRangeDistance))
				_desiredMoveSpeed = agent.properties.speed.max;
				
			
			// Check if too far away from the target or too
			// far away from where we started this attack state
			if ((!Helpers.InRadius(transform, target, profile.attack.stopAttackFromTargetDistance))
					|| (!Helpers.InRadius(transform, _attackEnteredPosition, profile.attack.stopAttackFromStartDistance)))
				state = State.Idle;
			yield return null;
		}
	}

	protected override void OnHealthLost(Health.HealthChangedEventInfo info)
	{
		if (info.value > 10)
			agent.Stagger();
		base.OnHealthLost(info);
	}


	void OnGUI()
	{
		if (GUILayout.Button("Idle"))
			state = State.Idle;
		if (GUILayout.Button("Attack"))
			state = State.Attack;
		if (GUILayout.Button("Retreat"))
			state = State.Retreat;
		if (GUILayout.Button("Patrol"))
			state = State.Patrol;
	}*/
}
