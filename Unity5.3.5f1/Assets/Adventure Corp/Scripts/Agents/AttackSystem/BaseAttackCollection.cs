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
	float _attackInterval = 0.5f;
	public AttackDescriptor GetNextMeleeFromSequence(AttackController controller)
	{
		bool updated = false;

		if (controller.isAttacking)
		{
			if (controller.isPastYieldControlTime)
			{
				_lastAttackTime = Time.time;
				_meleeSequenceIndex++;
				updated = true;
			}
		}
		else
		{
			_lastAttackTime = Time.time;
			updated = true;
			if ((Time.time - _lastAttackTime) > _attackInterval)
			{
				Debug.Log("too slow");
				_meleeSequenceIndex = 0;
			}
			else
				_meleeSequenceIndex++;			
		}




		if (_meleeSequenceIndex >= meleeSequence.Count)
		{
			_meleeSequenceIndex = 0;
			updated = true;
		}

		if (updated)
		{
			string s = "";
			for (int i = 0; i <= _meleeSequenceIndex; i++)
				s += i.ToString() + "..... ";
			Debug.Log(s);
		}


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

