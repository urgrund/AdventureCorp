using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BaseAttackCollection
{
	[HideInInspector] 
    public AttackDescriptor melee1;
    public AttackDescriptor melee2;
    public AttackDescriptor melee3;
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
			AttackDescriptor[] arrary = new AttackDescriptor[6];
			arrary[0] = melee1;
			arrary[1] = melee2;
			arrary[2] = melee3;
			arrary[3] = ranged1;
			arrary[4] = ranged2;
			arrary[5] = special;
			return arrary;
		}
		else
			return allAsArray;
	}
}