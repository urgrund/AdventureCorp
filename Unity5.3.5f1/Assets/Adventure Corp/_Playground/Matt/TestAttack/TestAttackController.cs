using UnityEngine;
using System.Collections;

public class TestAttackController : MonoBehaviour
{
    public Animation animObj;
    public AttackChain[] chains;    

    void OnGUI()
    {
        if (chains == null)
            return;

        if (animObj == null)
            return;

        foreach (AttackChain c in chains)
        {
            if (c == null)
                return;

            if (GUILayout.Button(c.name))
            {
                ExecuteAttackChain(c);
            }
        }
    }


    bool isChainRunning = false;
    void ExecuteAttackChain(AttackChain chain)
    {
        if (!isChainRunning)
            StartCoroutine(AttackChainRoutine(chain));   
    }

    IEnumerator AttackChainRoutine(AttackChain chain)
    {
        isChainRunning = true;
        foreach (AttackDescriptor ad in chain.attacks)
        {
            foreach (AnimationClipProperties p in ad.animations)
            {
                animObj.CrossFade(p.clip.name, p.blendTime, p.playMode);
                while (animObj.IsPlaying(p.clip.name))
                    yield return null;                
            }
        }
        isChainRunning = false;
    }


}
