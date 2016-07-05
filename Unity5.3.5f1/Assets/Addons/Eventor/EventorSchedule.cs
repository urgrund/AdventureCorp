using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EventorSchedule : MonoBehaviour
{


	// --------------------------------------------------------------------------------------------
	// S T A T I C   F U N C T I O N S 

	public static void Run(EventorSchedule schedule)
	{
		(Instantiate(schedule) as EventorSchedule).Run();
	}
    
    public static void RunAtLocation(EventorSchedule schedule, Vector3 position) { RunAtLocation(schedule, position, schedule.transform.rotation); }
    public static void RunAtLocation(EventorSchedule schedule, Transform transform) { RunAtLocation(schedule, transform.position, transform.rotation); }
	public static void RunAtLocation(EventorSchedule schedule, Vector3 position, Quaternion rotation)
	{
		(Instantiate(schedule, position, rotation) as EventorSchedule).Run();
	}   

	public static void RunAtTransformAsChild(EventorSchedule schedule, Transform transform)
	{ 
		EventorSchedule e = Instantiate(schedule, transform.position, transform.rotation) as EventorSchedule;
		e.transform.parent = transform;
		e.Run();
	}






	// --------------------------------------------------------------------------------------------




	public delegate void OnScheduleComplete();
	public event OnScheduleComplete onScheduleComplete;

	[HideInInspector]
	public bool isActive = true;

	[HideInInspector]
	public bool isStartOnAwake = true;

	[HideInInspector]
	public bool isRepeatInfinite = false;
	[HideInInspector]
	public EventorJobRepeatDetails repeat;
	[HideInInspector]
	public bool isDestroyOnAllCompelte = false;

	List<EventorJob> _jobs;
    public List<EventorJob> jobs { get { return _jobs; } }

	[HideInInspector]
	public bool isRunning = false;

	bool isRanOnce = false;
	int completions = 0;
	bool isCompletedAllRepeats = true;

	/// <summary>
	/// Current number of jobs running 
	/// </summary>
	int totalJobsRunning = 0;

	/// <summary>
	/// Jobs that have been fired
	/// </summary>
	int totalJobsStarted = 0;

	/// <summary>
	/// The total amount of jobs to open including repeats
	/// </summary>
	int totalJobsToOpen = 0;

	/// <summary>
	/// All jobs have have been ran once by the schedule
	/// </summary>
	bool isRanAllJobsOnce = false;

	void OnEnable()
	{
        FindJobs();
	}

	void Start()
	{
		if(isStartOnAwake)
			Run();
	}


    void FindJobs()
    {
        _jobs = new List<EventorJob>(GetComponentsInChildren<EventorJob>());        
    }

	/// <summary>
	/// Run this schedule and all child jobs
	/// </summary>
	public void Run()
	{
        // No jobs to run
		if(_jobs.Count == 0)
			return;

		// If this schedule loops indefinately
		// then don't allow another public Run
		if(repeat.isActive && isRepeatInfinite && isRanOnce)
			return;

		if(repeat.isActive && !isCompletedAllRepeats)
			return;
                
        // Subsribe delegate for looping
		//if(repeat.isActive)
		{
			onScheduleComplete -= OnScheduleCompleted;
			isCompletedAllRepeats = false;
			completions = 0;
			onScheduleComplete += OnScheduleCompleted;
		}

        if (!isRunning && isActive)
        {
            StartCoroutine(RunScheduleRoutine(0));
        }
	}
	




	void RunAllJobs()
	{
		for(int i=0; i<_jobs.Count; i++)
		{
			if(_jobs[i].isActive)
			{	
				// Clear delegates then resubscribe
				// each time the job is fired 
				_jobs[i].ClearDelegates();
				_jobs[i].onJobComplete += OnJobComplete;
				_jobs[i].onJobBegin += OnJobBegin;
				_jobs[i].Run();
			}
		}
		isRanOnce = true;
	}


	bool isAllJobsComplete { get { return (totalJobsRunning == 0 && isRanAllJobsOnce); } }

	void OnJobBegin(EventorJob job)
	{
		totalJobsStarted++;
		totalJobsRunning++;

		// All jobs have started now
		if(totalJobsStarted == totalJobsToOpen)
			isRanAllJobsOnce = true;
	}

	void OnJobComplete(EventorJob job)
	{
		totalJobsRunning--;
		if(isAllJobsComplete)		
		{		
			isRunning = false;
			System.GC.Collect();

		//	Debug.Log (this.name + " [ All Jobs Complete ]");

			if(onScheduleComplete != null)
				onScheduleComplete();
		}
	}

	void OnScheduleCompleted()
	{
		if(repeat.isActive)
		{
			completions++;

			if(!isCompletedAllRepeats)
			{
				if(completions < repeat.count || isRepeatInfinite)
				{
					StopAllCoroutines();
					StartCoroutine(RunScheduleRoutine(Random.Range(repeat.repeatDelay.x, repeat.repeatDelay.y)));
				}
				else
				{
					isCompletedAllRepeats = true;

					if(isDestroyOnAllCompelte)
						Destroy(this.gameObject);
				}
			}
		}
		else
		{
			if(isDestroyOnAllCompelte)
				Destroy(this.gameObject);
		}
	}

	void SetJobCounters()
	{
		//print ("resetting details");
		isRanAllJobsOnce = false;
		totalJobsRunning = 0;
		totalJobsStarted = 0;
		totalJobsToOpen = _jobs.FindAll(p => p.isActive == true).Count;
	}


	IEnumerator RunScheduleRoutine(float delay)
	{        
		SetJobCounters();
		isRunning = true;
		if(delay > 0f)
			yield return new WaitForSeconds(delay);
		RunAllJobs();
		yield return null;
	}

}























