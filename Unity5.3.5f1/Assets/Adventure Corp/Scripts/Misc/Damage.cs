﻿using UnityEngine;
using System.Collections;

public enum DamageType
{
    Generic,
    Fire,
    Ice,
    Piercing
}


[CreateAssetMenu(fileName = "dmg_Damage", menuName = "Adventure Corp/Damage")]
public class Damage : ScriptableObject
{ 
    public DamageType type = DamageType.Generic;
    public int amount = 1;
    public bool isDamageOverTime = false;
    public float damageOverTimeFrequency = 1f;
    public float damageOverTimeDuration = 10f;
}