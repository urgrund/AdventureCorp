using UnityEngine;
using System.Collections;

public class EventorAudio : EventorJob
{
	public AudioClip clip;
	public Transform target;

	protected override IEnumerator RunJobRoutine ()
	{
		if(clip != null && target != null)
		{
			if(!target.GetComponent<AudioSource>())			
				target.gameObject.AddComponent<AudioSource>();
			target.GetComponent<AudioSource>().PlayOneShot(clip);
		}
		yield return null;
	}
}
