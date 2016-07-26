using UnityEngine;
using System.Collections;

public class PatrolPoint : MonoBehaviour
{
    public bool isBusy;
  //  public float radius = 2;

    void OnDrawGizmos()
    {
        if (isBusy)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawSphere(transform.position, 0.2f);

        Helpers.GizmoDrawRing(transform.position, 1);
    }

}
