using UnityEngine;
using System.Collections;


public class NPCBat : NPCBrain
{
    Health playerToAttack;

    protected override void Awake()
    {
        playerToAttack = LevelManager.players[0];        
        base.Awake();
    }	


	protected override IEnumerator LogicRoutine()
	{
		yield return new WaitForSeconds(Random.Range(1, 3f));

		if (playerToAttack != null)
			destination = playerToAttack.transform.position;		

		if (Helpers.InRadius(transform.position, playerToAttack.transform.position, 2f))
			attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);		
	}
}
