using UnityEngine;
using System.Collections;

public class EventorDestroy : EventorJob
{
	public float delayLength;

	protected override IEnumerator RunJobRoutine ()
	{
		yield return new WaitForSeconds(delayLength);
        Destroy(gameObject);
	}
}
