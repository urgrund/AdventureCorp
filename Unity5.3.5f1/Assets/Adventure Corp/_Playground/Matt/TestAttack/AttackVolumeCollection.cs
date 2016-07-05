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
    public float capsuleHeight = 1f;
    [Range(0,2)]
    public int capsuleDirection;
}


// Stores a list of descriptors for attack volumes
public class AttackVolumeCollection : MonoBehaviour
{
    public AttackVolumeDescriptor[] volumes;
}
