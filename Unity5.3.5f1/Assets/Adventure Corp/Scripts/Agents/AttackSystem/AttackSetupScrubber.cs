using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AttackSetupScrubber : MonoBehaviour
{
    public Animation animatedGO;
    public AttackDescriptor attackDescriptor;
    public AttackVolumeCollection attackVolumeCollection;
    public Agent agent;
    public float scrubTime = 0;


    void OnDrawGizmos()
    {
        // Draw valid damage volume attachments
        if (attackVolumeCollection != null)
        {            
            foreach (AttackVolumeDescriptor d in attackVolumeCollection.volumes)
            {   
                Transform tf = Helpers.SearchHierarchyForTransform(animatedGO.transform, d.boneName);
                if (tf != null)
                {
                    Vector3 p = tf.TransformPoint(d.center);

                    if (attackDescriptor.volumes.Contains(d))
                    {
                        float t = animatedGO[animatedGO.clip.name].normalizedTime;
                        if (t < attackDescriptor.validDamageRange.y && t > attackDescriptor.validDamageRange.x)
                        {
                            Gizmos.color = Color.red * new Color(1, 1, 1, 0.25f);
                            Gizmos.DrawSphere(p, d.radius);
                            DrawWireSphere(p, d.radius, Color.red);                            
                        }
                        else
                        {
                            DrawWireSphere(p, d.radius, Color.cyan);
                        }
                    }
                    else
                        DrawWireSphere(p, d.radius, Color.grey);
                }
            }
        }       
    }

    void DrawWireSphere(Vector3 p, float r, Color c)
    {
        Color col = Gizmos.color;
        Gizmos.color = c;
        Gizmos.DrawWireSphere(p, r);
        Gizmos.color = col;
    }
}
