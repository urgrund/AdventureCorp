using UnityEngine;
using System.Collections;

public enum PlatformMoverState
{
	WaitingAtStart,
	WaitingAtDestination,
	MovingTo,
	MovingFrom
}

public enum MoverDetailsEasingType
{
	Linear,
	EaseIn,
	EaseOut,
	EaseInOut,
	ElasticIn,
	ElasticOut,
	BounceIn,
	BounceOut
}



[System.Serializable]
public class MoverDetails
{
    public bool delayStartByWaitTime = false;
    public bool isRandomStartPosition = false;

    public PlatformMoverLocationDetails start;
    public PlatformMoverLocationDetails target;
    public MoverDetailsSpin spin; 	   
}


[System.Serializable]
public class MoverDetailsSpin
{
    public bool isSpinToTarget = false;
    public float spinRevolutions;
    public Vector3 spinAxis = Vector3.up;
}


[System.Serializable]
public class PlatformMoverLocationDetails
{
	public string name;

    public bool toggleOnly = false;
    
    // Option to hook up a trigger to this mover
    // and listen for collider counts.  Useful for doors
    // or platforms where you expect something to be there to operate
    public bool toggleOnlyWithTrigger = false;
	public bool toggleCanInterupt = false;
    public Trigger trigger;
    public int triggerColliderCount = 1;

    // Time based values to wait at this position 
    // and how to transition to the next
    public float waitTime = 1f;
    public float timeToNextTarget = 1f;
    public MoverDetailsEasingType easingToNextTarget = MoverDetailsEasingType.EaseInOut;

    public EventorContainer onArriveEvent;


    [HideInInspector]
    public Vector3 positon = Vector3.zero;
    [HideInInspector]
    public Quaternion rotation = Quaternion.identity;

    private PlatformMover mover;

    public void Init(Transform target, PlatformMover platformMover)
    {
        if (platformMover == null)
        {
            Debug.LogError("Platformer was null?");
            return;
        }

        mover = platformMover;

        positon = target.position;
        rotation = target.rotation;

		if (toggleOnly && toggleOnlyWithTrigger)
		{
			//Debug.Log(this.ToString() + "  added delegate.");
			if (trigger != null)
				trigger.colliderCountChanged += this.OnColliderCountChanged;
		}

        mover.onArrived += OnArrived;
    }


	
	// This will subscribe to the triggers count delegate
	public void OnColliderCountChanged(int count)
	{
		// Not our message
		Debug.Log(name + " collider count trigger.");
		if (mover.currentDetails != this)
		{
			//Debug.Log("Wasn't us " + name);
			return;
		}
		

		if (mover.isCurrentlyMoving && !toggleCanInterupt)
			return;

		if (toggleOnly && toggleOnlyWithTrigger)			
			if (trigger != null)
				if (trigger.collidersInsideVolume == triggerColliderCount)
					mover.Toggle();
	}

    public void OnArrived(PlatformMover m, bool isMovingTo, PlatformMoverLocationDetails details)
    {
        // Not our message
        if (details != this)
            return;

		if (mover.isCurrentlyMoving && !toggleCanInterupt)
			return;

		if (toggleOnly && toggleOnlyWithTrigger)
            if (trigger != null)
                if (trigger.collidersInsideVolume == triggerColliderCount)
                    m.Toggle();
    }
}


