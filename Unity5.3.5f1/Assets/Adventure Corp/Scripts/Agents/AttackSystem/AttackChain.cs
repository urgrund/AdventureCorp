using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ac_AttackChain", menuName = "Adventure Corp/Attack Chain")]
public class AttackChain : ScriptableObject
{    
    public List<AttackDescriptor> attacks;   
}
