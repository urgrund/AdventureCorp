using UnityEngine;
using System.Collections;

[System.Serializable]
public class BaseAttackCollection
{
	[HideInInspector] 
    public AttackController controller;	// Auto assigned
    public AttackDescriptor melee1;
    public AttackDescriptor melee2;
    public AttackDescriptor melee3;
    public AttackDescriptor ranged1;
    public AttackDescriptor ranged2;
    public AttackDescriptor special;
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
