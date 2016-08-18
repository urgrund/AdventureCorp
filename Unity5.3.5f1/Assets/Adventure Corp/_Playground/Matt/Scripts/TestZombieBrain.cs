using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestZombieBrain : NPCBrain
{
	public Transform target;

	public Vector3 _spawnSpot;

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
		destination = transform.position;
		print("In Idle");
		while (true)
		{			
			yield return new WaitForSeconds(0.5f);
		}		
	}


	protected override IEnumerator UpdateRetreatState()
	{
		print("In Retreat");
		destination = _spawnSpot;
		while (true)
		{			
			yield return new WaitForSeconds(0.5f);
		}
	}


	protected override IEnumerator UpdatePatrolState()
	{
		print("In Patrol");
		while (true)
		{			
			yield return new WaitForSeconds(0.5f);
		}
	}	


	protected override IEnumerator UpdateAttackState()
	{
		print("In Attack");
		while (target != null)
		{
			List<AttackDescriptor> aDescs = _attackController.GetSuggestedAttacksForTarget(attackCollection, target);
			if (aDescs != null)
			{
				AttackDescriptor ad = aDescs[Random.Range(0, aDescs.Count)];
				_attackController.AttackWithDescriptor(ad);
			}
			else
			{
				destination = target.position;
				_desiredMoveSpeed = agent.properties.speed.max;				
			}
			yield return new WaitForSeconds(0.5f);
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
	}
}
