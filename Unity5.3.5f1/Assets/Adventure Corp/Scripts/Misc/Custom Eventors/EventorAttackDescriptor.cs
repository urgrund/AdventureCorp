using UnityEngine;
using System.Collections;
using System;

public class EventorAttackDescriptor : EventorJob
{
    public AttackDescriptor attack;
    public Animation animatedObject;    
    public Damager[] damageVolumes;

    float t;
    protected override IEnumerator RunJobRoutine()
    {
        ActivateDamageVolumes(false);
        if (animatedObject != null && attack != null)
        {            
            animatedObject.CrossFade(attack.clipProperties.clip.name, attack.clipProperties.blendTime, attack.clipProperties.playMode);
            while (animatedObject.IsPlaying(attack.clipProperties.clip.name))
            {
                t = animatedObject[attack.clipProperties.clip.name].normalizedTime;
                ActivateDamageVolumes(t > attack.validDamageRange.x && t < attack.validDamageRange.y);
                yield return null;
            }
        }
        ActivateDamageVolumes(false);
    }

    void ActivateDamageVolumes(bool isEnabled)
    {
        for (int i = 0; i < damageVolumes.Length; i++)
            damageVolumes[i].enabled = isEnabled;
    }
}


