using UnityEngine;
using System.Collections;

/// <summary>
/// Attack Setup Scrubber
/// Helper class only to use during edit time to 
/// help setup attack descriptors with visual feedback
/// </summary>
public class AttackSetupScrubber : MonoBehaviour
{
    public bool[] volumeIndices;
    public Vector3[] volumePositions; 
    public Animation animatedGO;
    public AttackDescriptor attackDescriptor;
    public AttackVolumeCollection attackVolumeCollection;
    public Agent agent;
    public float scrubTime = 0;


    public static Color c_inActive = Color.grey;
    public static Color c_inRange = Color.red;
    public static Color c_inUse = Color.cyan;

    void OnDrawGizmos()
    {
        // Draw valid damage volume attachments
        if (attackVolumeCollection != null)
        {
            volumePositions = new Vector3[volumeIndices.Length];
            for (int i = 0; i < volumeIndices.Length; i++)
            {
                AttackVolumeDescriptor d = attackVolumeCollection.volumes[i];
                Transform tf = Helpers.SearchHierarchyForTransform(animatedGO.transform, d.boneName);
                Vector3 p = tf.TransformPoint(d.center);
                volumePositions[i] = p;

                // This has been ticked for use in this attack 
                if (volumeIndices[i] == true)
                {
                    float t = animatedGO[animatedGO.clip.name].normalizedTime;
                    if (t < attackDescriptor.validDamageRange.y && t > attackDescriptor.validDamageRange.x)
                    {
                        Gizmos.color = c_inRange * new Color(1, 1, 1, 0.25f);
                        Gizmos.DrawSphere(p, d.radius);
                        DrawWireSphere(p, d.radius, c_inRange);
                    }
                    else
                    {
                        DrawWireSphere(p, d.radius, c_inUse);
                    }
                }
                else
                    DrawWireSphere(p, d.radius, c_inActive);
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
