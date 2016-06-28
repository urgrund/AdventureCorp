using UnityEngine;
using System.Collections;

public class PatrolPoint : MonoBehaviour
{
    [HideInInspector]
    public bool isBusy = false;

    public void SetStatus(bool b)
    {
        isBusy = b;
    }

    void OnDrawGizmos()
    {
        if (isBusy)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.blue;

        MathLab.GizmoDrawRing(transform.position, 1);
    }
}
