using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BaseAttackCollection
{
	[HideInInspector] 
    //public AttackController controller;	// Auto assigned
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



//
// Exmaple below how the collection can be extended
//


//public class GruntAttackCollection : BaseAttackCollection
//{
//    public float extraGruntPower = 3;
//}




//public class TestNPC
//{
//    public BaseAttackCollection collection;    
//}


//public class GruntNPC : TestNPC
//{
//	// override the underlaying collection 
//	public new GruntAttackCollection collection;
//}
