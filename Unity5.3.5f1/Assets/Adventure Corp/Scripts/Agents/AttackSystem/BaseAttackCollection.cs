using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ExplorerAttackCollection
{
	public List<AttackDescriptor> meleeSequence = new List<AttackDescriptor>();
	public AttackDescriptor ranged;
	public AttackDescriptor dash;
	public AttackDescriptor parry;

	int _meleeSequenceIndex = 0;
	float _lastAttackTime = 0;
	float _attackInterval = 1f;
	public AttackDescriptor GetNextMeleeFromSequence(AttackController controller)
	{
		if (controller.isAttacking)
		{
			if (controller.isPastYieldControlTime)
			{
				_lastAttackTime = Time.time;
				_meleeSequenceIndex++;
			}
		}
		else
		{
			if ((Time.time - _lastAttackTime) > _attackInterval)
				_meleeSequenceIndex = 0;
			else
				_meleeSequenceIndex++;
			_lastAttackTime = Time.time;
		}
		
		if (_meleeSequenceIndex >= meleeSequence.Count)		
			_meleeSequenceIndex = 0;

		return meleeSequence[_meleeSequenceIndex];
	}
}



[System.Serializable]
public class BaseAttackCollection
{	
    public AttackDescriptor melee1;
    public AttackDescriptor melee2;
    public AttackDescriptor melee3;
	public AttackDescriptor melee4;
	public AttackDescriptor melee5;
	public AttackDescriptor melee6;
	public AttackDescriptor ranged1;
    public AttackDescriptor ranged2;
    public AttackDescriptor special;

	// This is generated once at runtime
	private AttackDescriptor[] allAsArray = null;

	public AttackDescriptor[] AsArray()
	{
		// If the array wasn't generated
		if (allAsArray == null)
		{
			AttackDescriptor[] arrary = new AttackDescriptor[9];
			arrary[0] = melee1;
			arrary[1] = melee2;
			arrary[2] = melee3;
			arrary[3] = melee4;
			arrary[4] = melee5;
			arrary[5] = melee6;
			arrary[6] = ranged1;
			arrary[7] = ranged2;
			arrary[8] = special;
			return arrary;
		}
		else
			return allAsArray;
	}
}

