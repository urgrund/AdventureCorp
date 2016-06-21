using UnityEngine;
using System.Collections;

public class EventorAnimation : EventorJob
{
	public AnimationClip clip;
	public PlayMode playMode;
	public Transform target;

	bool _foundAnimComponent = false;


	void Awake()
	{
		if(target != null && clip != null)
		{
			if(target.GetComponent<Animation>())
			{
				target.GetComponent<Animation>().AddClip(clip, clip.ToString());
				_foundAnimComponent = true;
			}
			else
			{
				target.gameObject.AddComponent<Animation>();
				target.GetComponent<Animation>().AddClip(clip, clip.ToString());
				_foundAnimComponent = true;
			}
		}
	}


	protected override IEnumerator RunJobRoutine ()
	{	
		if(target != null && clip != null && _foundAnimComponent)
		{				
			target.GetComponent<Animation>().clip = clip;
			target.GetComponent<Animation>().Play(clip.ToString(), playMode);
			yield return null;
		}						
	}
}
