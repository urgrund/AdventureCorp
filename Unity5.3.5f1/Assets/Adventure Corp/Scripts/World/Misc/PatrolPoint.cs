﻿using UnityEngine;
using System.Collections;

public class PatrolPoint : MonoBehaviour
{
    public bool isBusy;
  //  public float radius = 2;

    void OnDrawGizmos()
    {
		Color alpha = new Color(1, 1, 1, 0.25f);
        if (isBusy)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

		Gizmos.color = Gizmos.color * alpha;

        Gizmos.DrawSphere(transform.position, 0.2f);
        Helpers.Draw.GizmoRing(transform.position, 1);
    }

}
