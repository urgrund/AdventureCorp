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

    protected override void Start()
    {
        StartCoroutine(UpdateRoutine());
        base.Start();
    }


    IEnumerator UpdateRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0, 2.5f));

        while (true && !agent.health.isDead)
        {
            if (playerToAttack != null)
            {
                destination = playerToAttack.transform.position;
            }

            if (Helpers.InRadius(transform.position, playerToAttack.transform.position, 2f))
                attackCollection.controller.AttackWithDescriptor(attackCollection.melee1);

            yield return new WaitForSeconds(1f);
        }
    }
}
