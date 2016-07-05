using UnityEngine;
using System.Collections;

public class EventorDelay : EventorJob
{
	public float delayLength;

	protected override IEnumerator RunJobRoutine ()
	{
		yield return new WaitForSeconds(delayLength);
	}
}
