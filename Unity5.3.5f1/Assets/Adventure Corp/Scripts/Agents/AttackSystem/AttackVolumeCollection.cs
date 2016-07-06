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

    public static Damager[] CreateDamageCollidersForTransform(Transform transform, AttackVolumeCollection attackVolumes)
    {
        Damager[] d = new Damager[attackVolumes.volumes.Length];
        for (int i = 0; i < attackVolumes.volumes.Length; i++)
        {
            GameObject go = new GameObject("Damager_" + attackVolumes.volumes[i].boneName);            
            Transform parent = Helpers.SearchHierarchyForTransform(transform, attackVolumes.volumes[i].boneName);
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
                }
                d[i] = damager;
            }            
        }
        return d;
    }
}
