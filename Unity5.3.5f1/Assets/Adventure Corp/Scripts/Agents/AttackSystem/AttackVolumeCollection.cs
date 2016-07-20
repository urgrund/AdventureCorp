using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttackVolumeDescriptor
{
    public enum Shape
    {
        Sphere,
        Capsule
    }
        
    public string boneName = "root";
    public Shape shape;
    public float radius = 1f;
    public Vector3 center;
    //public float capsuleHeight = 1f;
    //[Range(0,2)]
    //public int capsuleDirection;
}


// Stores a list of descriptors for attack volumes
public class AttackVolumeCollection : MonoBehaviour
{
    public AttackVolumeDescriptor[] volumes;


    /// <summary>
    /// Really slow approach...  be good to optimise this
    /// </summary>    
    public static Damager[] CreateDamageCollidersForAgent(Agent agent, Transform animatedObject, AttackVolumeCollection attackVolumes)
    {
        if (attackVolumes == null || attackVolumes.volumes == null)
        {
            Debug.LogError("No volumes created!");
            return null;
        }

        Damager[] d = new Damager[attackVolumes.volumes.Length];
        for (int i = 0; i < attackVolumes.volumes.Length; i++)
        {
            GameObject go = new GameObject("Damager_" + attackVolumes.volumes[i].boneName);            
            Transform parent = Helpers.SearchHierarchyForTransform(animatedObject, attackVolumes.volumes[i].boneName);
            if (parent != null)
            {
                Helpers.ParentAndCenterOnTransform(go.transform, parent);
                Damager damager = go.AddComponent<Damager>();
                damager.enabled = false;

                Rigidbody rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;

                if (attackVolumes.volumes[i].shape == AttackVolumeDescriptor.Shape.Sphere)
                {
                    SphereCollider sc = go.AddComponent<SphereCollider>();
                    sc.radius = attackVolumes.volumes[i].radius;
                    sc.center = attackVolumes.volumes[i].center;
                    sc.isTrigger = true;
                    sc.enabled = false;
                    damager.sphereCollider = sc;
                    damager.owner = agent.health;                    
                }
                d[i] = damager;
            }            
        }
        return d;
    }
}
