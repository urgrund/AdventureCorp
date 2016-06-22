using UnityEngine;
using System.Collections;

[System.Serializable]
public class CameraJobTimedDetails
{
	public bool isTimed = false;
	public float duration = 0;
}

[System.Serializable]
public class CameraJobTransitionDetails
{
	public float transitionToJobTime;
	public bool isImmediate;
}

[System.Serializable]
public class CameraJob
{
	public string targetName;
	public Vector3 targetOffset;
	public Vector3 position;
	public float smoothingPosition;
	public 	float smoothingRotation;
	public bool isRelative;

	public CameraJobTimedDetails timedDetails;
	public CameraJobTransitionDetails transition;

	public CameraJob(){}

	public CameraJob(CameraJob job)
	{
		this.timedDetails = new CameraJobTimedDetails();
		this.transition = new CameraJobTransitionDetails();

		this.targetName = job.targetName;
		this.targetOffset = job.targetOffset;
		this.position = job.position;
		this.smoothingPosition = job.smoothingPosition;
		this.smoothingRotation = job.smoothingRotation;
		this.isRelative = job.isRelative;

		this.timedDetails.isTimed = job.timedDetails.isTimed;
		this.timedDetails.duration = job.timedDetails.duration;

		this.transition.isImmediate = job.transition.isImmediate;
		this.transition.transitionToJobTime = job.transition.transitionToJobTime;
	}	
}

