using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraSystem : MonoBehaviour
{
	private static CameraSystem _instance = null;
    public static CameraSystem instance { get { return _instance; } }

	public CameraJobContainer defaultJob;

	[HideInInspector]
	public CameraJob previousJob;
	[HideInInspector]
	public CameraJob currentJob;
	[HideInInspector]
	public CameraJob targetJob;


	private Vector3 _targetPosition;
    public static Vector3 targetPosition { get { return _instance._targetPosition; } }

	private Vector3 _targetLookAt;
    public static Vector3 targetLookAt { get { return _instance._targetLookAt; } }

	private GameObject _currentGOTarget = null;
    public static GameObject currentGOTarget { get { return _instance._currentGOTarget; } }

        
    /// <summary>
    /// Pause updating the camera system this frame. Useful if you wish to override some camera behaviour manually
    /// </summary>
    public static void DoNotUpdateThisFrame() { _instance._updateThisFrame = false;  }
    private bool _updateThisFrame = true;


	void Awake()
    {
		if(_instance != null)
			Debug.LogError("Already a " + this.GetType() + " in the scene!");

		_instance = this;
	}



	void Start()
	{

		if(defaultJob == null)
			return;

		// Target is the default
		targetJob = defaultJob.job;

		// Immediately set the camera up for the defult job
		SetTargetGameObject(defaultJob.job.targetName);
		Camera.main.transform.position = GetCameraPositionFromJob(defaultJob.job);
		SetCameraTransform(GetCameraPositionFromJob(defaultJob.job), GetLookAtPositionFromJob(defaultJob.job));

		// Run the default job
		RunCameraJob(defaultJob);
	}
	
	void LateUpdate()   
	{
        if (!_updateThisFrame)
        {
            _updateThisFrame = true;
            return;
        }

		if(defaultJob == null)
			return;

		if(Time.renderedFrameCount % 50 == 0)
		{
			System.GC.Collect();
		}

		_targetPosition = _targetLookAt = Vector3.zero;

		if(targetJob == null)		
			if(defaultJob != null)			
				targetJob = defaultJob.job;

		UpdateCurrentJob();
		SetCameraTransform(_targetPosition, _targetLookAt);
	}




	// --------------------------------------------------------------------------------------------------
	private void SetCameraTransform(Vector3 position, Vector3 lookAt)
	{
		Camera.main.transform.position = position;
		Camera.main.transform.rotation = Quaternion.LookRotation(lookAt, Vector3.up);
	}





	// --------------------------------------------------------------------------------------------------
	private void LerpJobs(CameraJob a, CameraJob b, float t)
	{
		currentJob.position = Vector3.Lerp(a.position, b.position, t);
		currentJob.targetOffset  = Vector3.Lerp(a.targetOffset, b.targetOffset, t);
		currentJob.smoothingPosition = Mathf.Lerp(a.smoothingPosition, b.smoothingPosition, t);
		currentJob.smoothingRotation = Mathf.Lerp(a.smoothingRotation, b.smoothingRotation, t);
	}





	// --------------------------------------------------------------------------------------------------
	private void SetImportantTargetDetailsToCurrent()
	{
		currentJob.targetName = targetJob.targetName;
		currentJob.transition.transitionToJobTime = targetJob.transition.transitionToJobTime;
		currentJob.transition.isImmediate = targetJob.transition.isImmediate;
		currentJob.timedDetails.isTimed = targetJob.timedDetails.isTimed;
		currentJob.isRelative = targetJob.isRelative;
	}





	// --------------------------------------------------------------------------------------------------
	private IEnumerator TransitionCameras()
	{
		SetImportantTargetDetailsToCurrent();
		float t = 0;

		// Set the position and rotation
		// of the camera immediately to the 
		// target job
		if(targetJob.transition.isImmediate)
		{
			t = 1f;
			// Set position first, as need it to be at the 
			// target in order to set an immediate lookat
			Camera.main.transform.position = GetCameraPositionFromJob(targetJob);
			SetCameraTransform(GetCameraPositionFromJob(targetJob), GetLookAtPositionFromJob(targetJob));

			// Lerp directly to the target (t=1)
			LerpJobs (previousJob, targetJob, t);
			yield return null;
		}

		// Blend between the previous 
		// and the current job
		while(true)
		{
			t += ( Time.deltaTime / targetJob.transition.transitionToJobTime );
			t = Mathf.Clamp01(t);
			LerpJobs (previousJob, targetJob, t);
			yield return null;
		}
	}





	// --------------------------------------------------------------------------------------------------
	private Vector3 GetLookAtPositionFromJob(CameraJob j)
	{
		if(_currentGOTarget == null)				
			if(!SetTargetGameObject(targetJob.targetName))
				return Camera.main.transform.forward;

		Vector3 lookAtOffset = _currentGOTarget.transform.position + j.targetOffset;
		Vector3 lookAtDir = (lookAtOffset - Camera.main.transform.position).normalized;

		return lookAtDir;
	}

	private Vector3 GetCameraPositionFromJob(CameraJob j)
	{
		if (j.isRelative && _currentGOTarget != null)				
			return _currentGOTarget.transform.position + j.position;		
		else
			return j.position;
	}






	// --------------------------------------------------------------------------------------------------
	private void UpdateCurrentJob()	
	{
		_targetLookAt = Vector3.Lerp (Camera.main.transform.forward, GetLookAtPositionFromJob(targetJob), Time.deltaTime * currentJob.smoothingRotation );
		_targetPosition = Vector3.Lerp(Camera.main.transform.position, GetCameraPositionFromJob(targetJob), Time.deltaTime * currentJob.smoothingPosition);
	}






	// --------------------------------------------------------------------------------------------------

	public static void RunCameraJob(CameraJobContainer j) { RunCameraJob(j.job); }
	public static void RunCameraJob(CameraJob j)
	{
		if(_instance != null)
			_instance.RunCameraJobInternal(j);
		else
			Debug.LogError("Camera System was NULL");
	}

	private void RunCameraJobInternal(CameraJob j)
	{
		// Setting the previous job and executing the new job
		StopAllCoroutines ();
		
		// Prepare all the jobs
		previousJob = new CameraJob(targetJob);
		currentJob = new CameraJob(targetJob);
		targetJob = j;// new CameraJob(j);
		
		// Attempt to find the scene objects
		SetTargetGameObject(targetJob.targetName);
		
		// If the job is timed, fire the coroutine
		if(targetJob.timedDetails.isTimed )				
			StartCoroutine (TimeForJob (targetJob.timedDetails.duration));
		
		// Start transitioning the cameras
		StartCoroutine(TransitionCameras());
	}



	// --------------------------------------------------------------------------------------------------
	private IEnumerator TimeForJob(float t)
	{
		// Wait for job time
		yield return new WaitForSeconds (t);

		// Done with the timed job, return to previous
		RunCameraJob(new CameraJob( previousJob ));
	}




	// --------------------------------------------------------------------------------------------------
	private bool SetTargetGameObject(string s)
	{
		GameObject t = GameObject.Find(s);

		if(t != null)
		{
			_currentGOTarget = t;
			return true;
		}
		else
		{			
			return false;
		}
	}


}









































