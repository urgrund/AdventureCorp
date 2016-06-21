using UnityEngine;
using System.Collections;


public abstract class EventorJob : MonoBehaviour
{
	public delegate void OnJobComplete(EventorJob eventorJob);
	public event OnJobComplete onJobComplete;

	public delegate void OnJobBegin(EventorJob eventorJob);
	public event OnJobBegin onJobBegin;

	public string jobName = "Eventor Job";
	public bool isActive = true;
	public float initialDelay = 0f;

	private float _jobStartTime;
	private float _jobEndTime;
    public float jobStartTime { get { return _jobStartTime; } }
    public float jobEndTime { get { return _jobEndTime; } }

	private bool _isRunning = false;

	[HideInInspector]
	public bool isExpandedInEditor = false;


	/// <summary>
	/// Every EventorJob must implement this co-routine
	/// </summary>
	protected abstract IEnumerator RunJobRoutine();


	private IEnumerator InitialDelay()
	{
		if(initialDelay > 0f)
			yield return new WaitForSeconds(initialDelay);

		_isRunning = true;
		_jobStartTime = Time.time;

		if(onJobBegin != null)
			onJobBegin(this);

		yield return StartCoroutine(RunJobRoutine());

		if(onJobComplete != null)
			onJobComplete(this);
	}

	public void ClearDelegates()
	{
		onJobBegin = null;
		onJobComplete = null;
	}

	/// <summary>
	/// Begin this EventorJob 
	/// </summary>
	public void Run()
	{
		if(!_isRunning && isActive)	
		{
			onJobComplete += EventorJobComplete;
			StartCoroutine(InitialDelay());	
		}
	}

	/// <summary>
	/// When the Job has completed this function will fire. 
	/// It is always added to the job complete delegate
	/// </summary>
	protected virtual void EventorJobComplete(EventorJob eventorJob)
	{
		_jobEndTime = Time.time;
		_isRunning = false;
	}

}
















