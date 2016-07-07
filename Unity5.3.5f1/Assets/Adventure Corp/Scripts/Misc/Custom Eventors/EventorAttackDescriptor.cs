using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventorAttackDescriptor : EventorJob
{
    public AttackDescriptor attack;
    public Animation animatedObject;
    public TestAttackController controller;      
        
    protected override IEnumerator RunJobRoutine()
    {
        // Setup volumes
        SetDamageToDamageVolumes();
        ActivateDamageVolumes(false);
                
        float t;
        // Play animation and activate volumes during damage range
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
        else
            Debug.LogWarning("Attack had no animated object or attack descriptor");

        // Deactivate
        ActivateDamageVolumes(false);
    }

    // Apply the appropriate Damage object to the volumes    
    void SetDamageToDamageVolumes()
    {
        for (int i = 0; i < controller.damagers.Length; i++)        
            controller.damagers[i].damage = attack.damage;                
    }


    // This assumes the attack indices match the NPC's damager indices
    // If there's a mistmatch it's most likely that the attack descriptor
    // was never opened in the editor and initialized
    void ActivateDamageVolumes(bool isEnabled)
    {
        for (int i = 0; i < controller.damagers.Length; i++)
        {
            if (isEnabled)
                controller.damagers[i].enabled = attack.volumeIndices[i];
            else
                controller.damagers[i].enabled = false;
        }            
    }
}


