using UnityEngine;
using System.Collections;


public class NPCSkeletonGrunt : NPCBrain
{
	protected override IEnumerator LogicRoutine()
	{
		yield return new WaitForSeconds(1f);
		yield return null;
	}
}
