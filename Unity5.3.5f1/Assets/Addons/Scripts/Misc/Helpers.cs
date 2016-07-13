using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Helpers
/// A generic helpers class with some common methods that could be used accross
/// any project.
/// Matt Bell (c) 2011
/// </summary>
public static class Helpers
{
    /// <summary>
    /// If a point lies within another point given a radius
    /// </summary>        
    public static bool InRadius(Vector3 p1, Vector3 p2, float radius) { return ((p1 - p2).sqrMagnitude < radius * radius); }


    /// <summary>
    /// If a point lies within another point given a radius. Assumes height of 0.
    /// </summary>    
    public static bool InRadiusGrounded(Vector3 p1, Vector3 p2, float radius) { return InRadius(new Vector3(p1.x, 0f, p1.z), new Vector3(p2.x, 0f, p2.z), radius); }


    /// <summary>
    /// Get the position on the plane at the given height where a ray would intersect
    /// </summary>    
    public static Vector3 QuickRayOnHorizonPlane(float planeHeight, Vector3 rayStart, Vector3 rayDir)
    {
        float angle = Vector3.Angle(-rayDir, Vector3.up);
        angle *= Mathf.Deg2Rad;
        float len = rayStart.y / Mathf.Cos(angle);
        return (rayStart + (rayDir * len));
    }


    public static Vector3 QuickRayOnZeroPlaneFromMainCamera(Vector3 screenPosition)
    {
        return Helpers.QuickRayOnHorizonPlane(0f, Camera.main.transform.position, -Camera.main.ScreenPointToRay(screenPosition).direction);
    }


    /// <summary>
    /// For three given vectors return true if they form a straight a line within the epsilon
    /// </summary>
    public static bool CheckStraightLine(Vector3 first, Vector3 second, Vector3 third, float angle)
    {
        Vector3 firstToLast = Vector3.Normalize(third - first);
        if (Vector3.Angle(firstToLast, Vector3.Normalize(second - first)) > angle)
            return false;
        return true;
    }


    /// <summary>
    /// Find a random spot near a target position that bias toward the source position
    /// This is useful if wanting to find were near a target you might want to move to
    /// </summary>    
    public static Vector3 GetRandomPositionNearTarget(Vector3 source, Vector3 target, bool isCloserSide, float distance)
    {
        float newdist = Random.Range(distance, -distance);
        Vector3 dir = Vector3.Normalize(target - source);
        Vector3 cross = Vector3.Cross(dir, Vector3.up) * newdist;
        return target + cross + (-dir * (distance - Mathf.Abs(newdist)));
    }





    public static List<T> ListResize<T>(List<T> list, int sz, T c = default(T))
    {
        int cur = list.Count;
        if (sz < cur)
            list.RemoveRange(sz, cur - sz);
        else if (sz > cur)
            list.AddRange(Enumerable.Repeat(c, sz - cur));
        return list;
    }     

    public static List<T> ListShuffle<T>(List<T> list)
    {
        if (list.Count > 1)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                T tmp = list[i];
                int randomIndex = (int)Random.value;
                list[i] = list[randomIndex];
                list[randomIndex] = tmp;
            }
        }
        return list;
    }



    /// <summary>
    /// Check to see if a position is off screen
    /// </summary>
    public static bool OffScreen(Vector3 worldPosition, float paddingFactor)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 pad = new Vector3((Screen.width * paddingFactor) / Screen.width, (Screen.height * paddingFactor) / Screen.height, 0f);
        screenPos.x /= Screen.width;
        screenPos.y /= Screen.height;
        if (screenPos.x + pad.x > 1 || screenPos.x - pad.x < 0 || screenPos.y + pad.y > 1 || screenPos.y - pad.y < 0)
            return true;
        return false;
    }
    public static bool OffScreen(Vector3 position) { return OffScreen(position, 0f); }


    /// <summary>
    /// Clamps a Vector3 within screen space bounds
    /// </summary>    
    public static Vector3 ClampPositionToScreen(Vector3 worldPosition, float paddingFactor)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        Vector3 pad = new Vector3((Screen.width * paddingFactor) / Screen.width, (Screen.height * paddingFactor) / Screen.height, 0f);
        
        screenPos.x /= Screen.width;
        screenPos.y /= Screen.height;
        if (screenPos.x + pad.x > 1)
            screenPos.x =  1 - pad.x;
        if (screenPos.x - pad.x < 0)
            screenPos.x = pad.x;
        if (screenPos.y + pad.y > 1)
            screenPos.y = 1 - pad.y;
        if (screenPos.y - pad.y < 0)
            screenPos.y = pad.y;

        return Camera.main.ScreenToWorldPoint(screenPos);
    }


    /// <summary>
    /// Draw a Gizmos ring oriented to the ground
    /// </summary>
    public static void GizmoDrawRing(Vector3 position, float radius)
    {
        int count = 32;
        Vector3 start, end;
        float div = (1 / (float)count) * Mathf.PI * 2;
        for (int i = 0; i < count; i++)
        {
            start = new Vector3(Mathf.Sin(div * i), 0f, Mathf.Cos(div * i)).normalized * radius;
            end = new Vector3(Mathf.Sin(div * (i + 1)), 0f, Mathf.Cos(div * (i + 1))).normalized * radius;
            Gizmos.DrawLine(position + start, position + end);
        }
    }


    public static Vector3 DirectionTo(Transform from, Vector3 to) { return DirectionTo(from.position, to); }
    public static Vector3 DirectionTo(Vector3 from, Transform to) { return DirectionTo(from, to.position); }
    public static Vector3 DirectionTo(Transform from, Transform to) { return DirectionTo(from.position, to.position); }
    public static Vector3 DirectionTo(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }


    /// <summary>
    /// Checks to see if a point is inside the view of a certain camera
    /// </summary>
    public static bool IsWorldPointInView(Vector3 point, Camera camera)
    {
        Vector3 p = camera.WorldToScreenPoint(point);
        if (p.x < 0 || p.x > Screen.width || p.y < 0 || p.y > Screen.height || p.z < camera.nearClipPlane || p.z > camera.farClipPlane)
            return false;
        return true;
    }


    public static void ParentAndCenterOnTransform(Transform transform, Transform parent)
    {
        transform.position = parent.position;
        transform.rotation = parent.rotation;
        transform.parent = parent;
    }

    /// <summary>
    /// Instantiates a prefab and parents it to another with the option to 
    /// snap it to the same position and rotation
    /// </summary>        
    public static Transform InstantiateAndParent(Transform transform, Transform parent, bool snapPositions)
    {
        transform = MonoBehaviour.Instantiate(transform);
        if (snapPositions)
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
        transform.parent = parent;        
        return transform;
    }



    /// <summary>
    /// Deep heirachy search for a transform where it appears Unity only goes to 
    /// 2nd level transform childs?
    /// </summary>    
    public static Transform SearchHierarchyForTransform(Transform current, string name)
    {
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name == name)
            return current;
        // search through child bones for the bone we're looking for
        for (int i = 0; i < current.childCount; ++i)
        {
            // the recursive step; repeat the search one step deeper in the hierarchy
            Transform found = SearchHierarchyForTransform(current.GetChild(i), name);
            // a transform was returned by the search above that is not null,
            // it must be the bone we're looking for
            if (found != null)
                return found;
        }

        // bone with name was not found
        return null;
    }


    public static Transform FindClosestTransform(Transform[] transforms, Transform testAgainst) { return FindClosestTransform(transforms, testAgainst.position); }
    public static Transform FindClosestTransform(Transform[] transforms, Vector3 testAgainst)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = testAgainst;
        foreach (Transform potentialTarget in transforms)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }
}