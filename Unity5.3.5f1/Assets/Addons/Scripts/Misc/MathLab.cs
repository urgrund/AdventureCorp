using UnityEngine;
using System.Collections;

public static class MathLab 
{

	//Projects a vector onto a plane. The output is not normalized.
	public static Vector3 ProjectVectorOnPlane(Vector3 planeNormal, Vector3 vector)
	{
		return vector - (Vector3.Dot(vector, planeNormal) * planeNormal);
	}

	//TODO
	//create a vector of direction "vector" with length "size"
	public static Vector3 SetVectorLength(Vector3 vector, float size)
	{
		
		//normalize the vector
		Vector3 vectorNormalized = Vector3.Normalize(vector);
		
		//scale the vector
		return vectorNormalized *= size;
	}

	//TODO
	//This function returns a point which is a projection from a point to a line.
	//The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
	{
		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;
		
		float t = Vector3.Dot(linePointToPoint, lineVec);
		
		return linePoint + lineVec * t;
	}

	//TODO
	//This function returns a point which is a projection from a point to a plane.
	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		
		float distance;
		Vector3 translationVector;
		
		//First calculate the distance from the point to the plane:
		distance = SignedDistancePlanePoint(planeNormal, planePoint, point);
		
		//Reverse the sign of the distance
		distance *= -1;
		
		//Get a translation vector
		translationVector = SetVectorLength(planeNormal, distance);
		
		//Translate the point to form a projection
		return point + translationVector;
	}

	//TODO
	//Get the shortest distance between a point and a plane. The output is signed so it holds information
	//as to which side of the plane normal the point is.
	public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		
		return Vector3.Dot(planeNormal, (point - planePoint));
	}
	

	public static float SinWave(float amplitude, float frequency, float phase, float time)
	{
		return amplitude*Mathf.Sin (time*frequency + phase);
	}
	
	public static float CosWave(float amplitude, float frequency, float phase, float time)
	{
		return amplitude*Mathf.Cos (time*frequency + phase);
	}

	/// <summary>
	/// Converts the vector relative to cam. Takes a bool that zeroes out the Y axis
	/// </summary>
	public static Vector3 ConvertVectorRelativeToCam(Vector3 v,bool isYAxisOn)
	{
		Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
		
		if(!isYAxisOn)
			forward.y = 0;
		
		forward = forward.normalized;
		Vector3 right = Vector3.Cross(forward,Vector3.down);
		
		v = v.z*forward + v.x*right;
		
		return v;
	}
	
	/// <summary>
	/// Creates a rotation to look at on the XZ axis
	/// </summary>
	public static Quaternion CreateRotationToLookAt(Vector3 target, Vector3 from)
	{
		target.y = from.y;
		Vector3 direction = (target - from).normalized;
        if (direction == Vector3.zero)
            return Quaternion.identity;
		Quaternion rotationToLookAt = Quaternion.LookRotation(direction, Vector3.up);
		return rotationToLookAt;
	}

	public static bool IsTargetInCone(Vector3 from,Vector3 target, float angle, Vector3 forward, float coneRadius)
	{
		Vector3 dir = GrabDirection(from,target);
		return (Vector3.Angle(forward, dir) <= angle && Vector3.Distance(from,target) <= coneRadius);
	}

	public static Vector3 GrabDirection(Vector3 from, Vector3 to)
	{
		to.y = 0;
		from.y = 0;
		Vector3 dir = (to - from).normalized;
		return dir;
	}

    public static bool IsTargetWithinDistane(Vector3 from, Vector3 target, float distance)
    {
        return (Vector3.Distance(from,target) <= distance);
    }

    //public static void GizmoDrawRing(Vector3 position, float radius)
    //{
    //    int count = 32;
    //    Vector3 start, end;
    //    float div = (1 / (float)count) * Mathf.PI * 2;
    //    for (int i = 0; i < count; i++)
    //    {
    //        start = new Vector3(Mathf.Sin(div * i), 0f, Mathf.Cos(div * i)).normalized * radius;
    //        end = new Vector3(Mathf.Sin(div * (i + 1)), 0f, Mathf.Cos(div * (i + 1))).normalized * radius;
    //        Gizmos.DrawLine(position + start, position + end);
    //    }
    //}
}
