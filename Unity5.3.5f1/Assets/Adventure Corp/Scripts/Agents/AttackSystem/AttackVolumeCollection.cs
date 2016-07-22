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
    public static Damager[] CreateDamageCollidersForAgent(AttackController attackController, Agent agent, Transform animatedObject, AttackVolumeCollection attackVolumes)
    {
		Debug.Assert(attackVolumes != null && attackVolumes.volumes != null, "Attack Volumes null! Cannot create.");

        Damager[] d = new Damager[attackVolumes.volumes.Length];
        for (int i = 0; i < attackVolumes.volumes.Length; i++)
        {
            GameObject go = new GameObject("Damager_" + attackVolumes.volumes[i].boneName);
			go.layer = agent.gameObject.layer;
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
					// Prepare sphere collider
                    SphereCollider spherecollider = go.AddComponent<SphereCollider>();
                    spherecollider.radius = attackVolumes.volumes[i].radius;
                    spherecollider.center = attackVolumes.volumes[i].center;
                    spherecollider.isTrigger = true;
                    spherecollider.enabled = false;

					// Peprare damager
					damager.sphereCollider = spherecollider;
                    damager.owner = agent.health;
					damager.attackController = attackController;
                }
                d[i] = damager;
            }            
        }
        return d;
    }
}
