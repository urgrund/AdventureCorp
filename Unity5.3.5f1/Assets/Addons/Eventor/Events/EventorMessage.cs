using UnityEngine;
using System.Collections;

[System.Serializable]
public class EventorMessagePackageDetails
{
	public string message;
	public GameObject target;
}

public class EventorMessage : EventorJob
{
	public EventorMessagePackageDetails[] messages;

	protected override IEnumerator RunJobRoutine ()
	{
		for(int i=0; i < messages.Length; i++)		
			if(messages[i].target != null)
				messages[i].target.BroadcastMessage(messages[i].message);
		yield return null;
	}
}
