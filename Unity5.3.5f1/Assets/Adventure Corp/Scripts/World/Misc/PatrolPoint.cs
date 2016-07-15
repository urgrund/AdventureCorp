using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PatrolPoint : MonoBehaviour
{
    [HideInInspector]
    public bool isBusy = false;

    [System.Serializable]
    public class Link
    {
        public PatrolPoint patrolPoint;
        public Vector3[] pathToLink;
    }


    public float radius = 2f;
    public Link[] links;

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

        Helpers.GizmoDrawRing(transform.position, radius);

        float h = 1f;
        Vector3 size = new Vector3(h * 0.2f, h, h * 0.2f);
        Vector3 offset = Vector3.up * h * 0.5f;
        Gizmos.DrawCube(transform.position + offset, size);
        Gizmos.DrawSphere(transform.position + Vector3.up * h, 0.3f);

        Gizmos.color = Color.red;
        foreach (Link l in links)
        {
            for (int i = 0; i < l.pathToLink.Length; i++)
            {
                if (i - 1 >= 0)
                    Gizmos.DrawLine(l.pathToLink[i], l.pathToLink[i - 1]);
                Gizmos.DrawSphere(l.pathToLink[i], 0.25f);
            }
        }
    }

    [ContextMenu("Generate Patrol Paths")]
    void GeneratePathsToPatrolLinks()
    {
        foreach (Link l in links)
        {            
            NavMeshPath path = new NavMeshPath();
            l.pathToLink = new Vector3[0];

            Vector2 f2 = Random.insideUnitCircle * radius;
            Vector3 from = transform.position +  new Vector3(f2.x, 0, f2.y);

            Vector3 t2 = Random.insideUnitCircle * l.patrolPoint.radius;
            Vector3 to = l.patrolPoint.transform.position +  new Vector3(t2.x, 0, t2.y);

            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {         
                l.pathToLink = path.corners;
            }
        }
    }

    //void OnGUI()
    //{
    //    if (GUILayout.Button("asdsd"))
    //        GeneratePathsToPatrolLinks();
    //}
        

}
