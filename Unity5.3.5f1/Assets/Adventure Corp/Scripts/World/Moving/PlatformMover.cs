using UnityEngine;
using System.Collections;
using System.Linq;

public class PlatformMover : MonoBehaviour
{
	public Transform mainTransform;
	public Transform targetTransform;		
    public MoverDetails details;

    public bool drawTargetMesh = true;
    public Color targetColor = Color.yellow;


	public bool isCurrentlyMoving = false;

    //private bool isFirstRun;

    // TODO - this can easily be an array in oder to 
    // have 'trains' of target loctaions
    private PlatformMoverLocationDetails _currentDetails;
    private PlatformMoverLocationDetails _nextDetails;
	public PlatformMoverLocationDetails currentDetails { get { return _currentDetails; } }
	public PlatformMoverLocationDetails nextDetails { get { return _nextDetails; } }


	public delegate void OnArrived(PlatformMover mover, bool isMovingTo, PlatformMoverLocationDetails details);
    public event OnArrived onArrived;
	
	void Start()
	{
        if (targetTransform == null)
        {
            Debug.LogError("No Target");
            return;
        }

        // Cahce the pos/rot of the mover and target        
        details.start.Init(mainTransform, this);
        details.target.Init(targetTransform, this);

        _currentDetails = details.start;
        _nextDetails = details.target;

        //isFirstRun = true;		
		StartCoroutine(MoveRoutine());
	}

	void OnDisable()
	{
		mainTransform.position = details.start.positon;
		StopAllCoroutines();
	}


    /// <summary>
    /// Return the float based on the easing function provided
    /// </summary>    
    float GetEasingFloat(MoverDetailsEasingType type, float t)
    {
        switch (type)
        {
            case MoverDetailsEasingType.EaseInOut:
                return Easing.Exponential.easeInOut(t);

            case MoverDetailsEasingType.EaseIn:
                return Easing.Exponential.easeIn(t);

            case MoverDetailsEasingType.EaseOut:
                return Easing.Exponential.easeInOut(t);

            case MoverDetailsEasingType.Linear:
                return Easing.Linear.easeInOut(t);

            case MoverDetailsEasingType.ElasticIn:
                return Easing.Elastic.easeIn(t);

            case MoverDetailsEasingType.ElasticOut:
                return Easing.Elastic.easeOut(t);

            case MoverDetailsEasingType.BounceIn:
                return Easing.Bounce.easeIn(t);

            case MoverDetailsEasingType.BounceOut:
                return Easing.Bounce.easeOut(t);
        }

        return t;
    }


    /// <summary>
    /// Based on a set of mover details, lock the mainTransform here.  This
    /// is good to garuntee a positoin instead of ending at a lerp of 0.001f etc
    /// </summary>    
    void LockTransformAtDetails(PlatformMoverLocationDetails detail, bool setSpin)
    {
        mainTransform.position = detail.positon;
        mainTransform.rotation = detail.rotation;

        if (setSpin)
            if (details.spin.isSpinToTarget)
                mainTransform.Rotate(details.spin.spinAxis, details.spin.spinRevolutions * 360f);
    }


    private bool _toggledThisFrame = false;
    public void Toggle()
    {
        _toggledThisFrame = true;
    }	

	void ResetToggle()
	{
		_toggledThisFrame = false;
	}


	private void SwitchDetails()
	{
		// Switch the previous/next details
		_nextDetails = isMovingTo ? details.start : details.target;
		_currentDetails = isMovingTo ? details.target : details.start;
		isMovingTo = !isMovingTo;
	}




    float t = 0;        // interpolater
    float r = 0;        // spin interpolater    
    bool isMovingTo;
    IEnumerator MoveRoutine()
    {
        isMovingTo = true;

        if (_currentDetails.toggleOnly)
        {
            while (!_toggledThisFrame)
                yield return null;
            ResetToggle();
        }
        else
            if(details.delayStartByWaitTime)
                yield return new WaitForSeconds(_currentDetails.waitTime);


        while (true)
        {
			t = 0f;
			while (t < 1f)
            {
                t += Time.deltaTime * (1f / _currentDetails.timeToNextTarget);

                mainTransform.position = Vector3.LerpUnclamped(_currentDetails.positon, _nextDetails.positon, GetEasingFloat(_currentDetails.easingToNextTarget, t));
                mainTransform.rotation = Quaternion.LerpUnclamped(_currentDetails.rotation, _nextDetails.rotation, GetEasingFloat(_currentDetails.easingToNextTarget, t));

                // if spin, then add additional rotation each frame
                if (details.spin.isSpinToTarget)
                {
                    r = Mathf.Lerp(0, (details.spin.spinRevolutions * 360f), GetEasingFloat(_currentDetails.easingToNextTarget, t));
                    mainTransform.Rotate(details.spin.spinAxis, r);
                }


				// This works, but needs a smarter way to switch 
				// because of the literal position that the easing 
				// functions create in worldspace
				//if (_toggledThisFrame)
				//{
				//	if (_nextDetails.toggleOnly && _nextDetails.toggleCanInterupt)
				//	{
				//		t = 1 - t;
				//		r = 1 - t;
				//		SwitchDetails();
				//	}
				//	ResetToggle();
				//}

                yield return null;
            }


			// Arrived, so fire off any events
			//Arrived(_currentDetails, isMovingTo);
			if (onArrived != null)
				onArrived(this, isMovingTo, _currentDetails);

			if (_currentDetails.onArriveEvent != null)
				_currentDetails.onArriveEvent.Run();

			// Snap the transform to the next details as there 
			// may have been a little difference based on delta time
			LockTransformAtDetails(_nextDetails, isMovingTo);

			SwitchDetails();
			

			// Wait for this details delay time
			yield return new WaitForSeconds(_currentDetails.waitTime);			

			// Check for toggling
			if (_currentDetails.toggleOnly)
            {
				if (_currentDetails.toggleOnlyWithTrigger)
				{
					if (_currentDetails.trigger != null)
						if (_currentDetails.trigger.collidersInsideVolume == _currentDetails.triggerColliderCount)
							Toggle();
				}				
				
				while (!_toggledThisFrame)
					yield return null;
				
                ResetToggle();
            }                        
        }
    }



	void OnGUI()
	{
		if (GUILayout.Button("Toggle"))
			Toggle();
	}

    



    // -------------------------------------------------------------------------------------------------
    // GIZMOS 
    void GizmoDrawing(Color c, float alpha, bool drawMesh)
    {
        if (targetTransform == null || mainTransform == null)
            return;

        c.a = alpha;
        Gizmos.color = c;

        Vector3 t = mainTransform.position;
        Vector3 tar = targetTransform.position;
        Quaternion tR = mainTransform.rotation;
        Quaternion tarR = targetTransform.rotation;

        if (Application.isPlaying)
        {
            t = details.start.positon;
            tR = details.start.rotation;

            tar = details.target.positon;
            tarR = details.target.rotation;
        }


        Gizmos.DrawLine(t, tar);

        if (drawMesh)
        {
            if (!mainTransform.GetComponent<MeshFilter>())
                return;

            Gizmos.DrawWireMesh(mainTransform.GetComponent<MeshFilter>().sharedMesh,
                tar, tarR, mainTransform.localScale);
            Gizmos.DrawWireMesh(mainTransform.GetComponent<MeshFilter>().sharedMesh,
                t, tR, mainTransform.localScale);            
        }
        else
        {
            Gizmos.DrawCube(tar, Vector3.one * 0.5f);
        }
    }

    void OnDrawGizmosSelected ()
    {
        GizmoDrawing(targetColor, 0.5f, drawTargetMesh);
    }

	void OnDrawGizmos()
	{
        GizmoDrawing(targetColor, 1f, false);

	}      
}

