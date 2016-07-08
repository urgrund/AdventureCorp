using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventorAttackDescriptor : EventorJob
{
    public AttackDescriptor attack;
    public Animation animatedObject;
    public AttackController controller;      
        
    protected override IEnumerator RunJobRoutine()
    {
        // Setup volumes
        controller.SetDamageToDamageVolumes(attack.damage);
        controller.ActivateDamageVolumes(false, attack.volumeIndices);
                
        float t;
        // Play animation and activate volumes during damage range
        if (animatedObject != null && attack != null)
        {
            animatedObject.CrossFade(attack.clipProperties.clip.name, attack.clipProperties.blendTime, attack.clipProperties.playMode);
            while (animatedObject.IsPlaying(attack.clipProperties.clip.name))
            {
                t = animatedObject[attack.clipProperties.clip.name].normalizedTime;
                controller.ActivateDamageVolumes(t > attack.validDamageRange.x && t < attack.validDamageRange.y, attack.volumeIndices);
                yield return null;
            }
        }
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        // Deactivate
        controller.ActivateDamageVolumes(false, attack.volumeIndices);
    }       
}


