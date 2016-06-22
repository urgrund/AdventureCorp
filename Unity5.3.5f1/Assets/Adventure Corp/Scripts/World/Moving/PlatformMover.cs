using UnityEngine;
using System.Collections;
using System.Linq;

public class PlatformMover : MonoBehaviour
{
	public Transform targetTransform;		
    public MoverDetails details;

    public bool drawTargetMesh = true;
    public Color targetColor = Color.yellow;
    

    private bool isFirstRun;

    // TODO - this can easily be an array in oder to 
    // have 'trains' of target loctaions
    private PlatformMoverLocationDetails _previousDetails;
    private PlatformMoverLocationDetails _nextDetails;

    public delegate void OnArrived(PlatformMover mover, bool isMovingTo, PlatformMoverLocationDetails details);
    public event OnArrived onArrived;

    void Arrived(PlatformMoverLocationDetails details, bool setSpin)
    {
        t = 0f;

        if (onArrived != null)
            onArrived(this, isMovingTo, details);

        if (details.onArriveEvent != null)
            details.onArriveEvent.Run();

        LockTransformAtDetails(details, setSpin);
    }


	void OnEnable()
	{
        if (targetTransform == null)
        {
            Debug.LogError("No Target");
            return;
        }

        // Cahce the pos/rot of the mover and target
        //details.start.positon = transform.position;
        //details.start.rotation = transform.rotation;
        //details.target.positon = targetTransform.position;
        //details.target.rotation = targetTransform.rotation;
        details.start.Init(transform, this);
        details.target.Init(targetTransform, this);

        _previousDetails = details.start;
        _nextDetails = details.target;

        isFirstRun = true;
		StartCoroutine(MoveRoutine());
	}

	void OnDisable()
	{
		transform.position = details.start.positon;
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
    /// Based on a set of mover details, lock the transform here.  This
    /// is good to garuntee a positoin instead of ending at a lerp of 0.001f etc
    /// </summary>    
    void LockTransformAtDetails(PlatformMoverLocationDetails detail, bool setSpin)
    {
        transform.position = detail.positon;
        transform.rotation = detail.rotation;

        if (setSpin)
            if (details.spin.isSpinToTarget)
                transform.Rotate(details.spin.spinAxis, details.spin.spinRevolutions * 360f);
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


    float t = 0;        // interpolater
    float r = 0;        // spin interpolater    
    bool isMovingTo;
    IEnumerator MoveRoutine()
    {
        isMovingTo = true;

        if (_previousDetails.toggleOnly)
        {
            while (!_toggledThisFrame)
                yield return null;
            ResetToggle();
        }
        else
            if(details.isWaitFirstTime)
                yield return new WaitForSeconds(_previousDetails.waitTime);


        while (true)
        {
            while (t < 1f)
            {
                t += Time.deltaTime * (1f / _previousDetails.timeToNextTarget);

                transform.position = Vector3.LerpUnclamped(_previousDetails.positon, _nextDetails.positon, GetEasingFloat(_previousDetails.easingToNextTarget, t));
                transform.rotation = Quaternion.LerpUnclamped(_previousDetails.rotation, _nextDetails.rotation, GetEasingFloat(_previousDetails.easingToNextTarget, t));

                // if spin, then add additional rotation each frame
                if (details.spin.isSpinToTarget)
                {
                    r = Mathf.Lerp(0, (details.spin.spinRevolutions * 360f), GetEasingFloat(_previousDetails.easingToNextTarget, t));
                    transform.Rotate(details.spin.spinAxis, r);
                }

                yield return null;
            }


            // Arrived, so fire off any events
            Arrived(_nextDetails, isMovingTo);

            // If this target needs to wait for a toggle
            // then loop here otherwise wait for time
            if (_nextDetails.toggleOnly)
            {
                while(!_toggledThisFrame)
                    yield return null;
                ResetToggle();
            }
            else
                yield return new WaitForSeconds(_nextDetails.waitTime);

            // Switch the previous/next details
            if(isMovingTo)
            {                
                _nextDetails = details.start;
                _previousDetails = details.target;
            }
            else            
            {
                _nextDetails = details.target;
                _previousDetails = details.start;
            }
            isMovingTo = !isMovingTo;           
        }
    }

    

    void Reset()
    {
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().mass = 100;
        }
    }




    // -------------------------------------------------------------------------------------------------
    // GIZMOS 
    void GizmoDrawing(Color c, float alpha, bool drawMesh)
    {
        if (targetTransform == null)
            return;

        c.a = alpha;
        Gizmos.color = c;

        Vector3 t = transform.position;
        Vector3 tar = targetTransform.position;
        Quaternion tR = transform.rotation;
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
            if (!GetComponent<MeshFilter>())
                return;

            Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh,
                tar, tarR, transform.localScale);
            Gizmos.DrawWireMesh(GetComponent<MeshFilter>().sharedMesh,
                t, tR, transform.localScale);            
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

