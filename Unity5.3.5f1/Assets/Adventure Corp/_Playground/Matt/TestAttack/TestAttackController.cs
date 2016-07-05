using UnityEngine;
using System.Collections;

public class TestAttackController : MonoBehaviour
{
    public Animation animObj;
    public AttackChain[] chains;

    public EventorSchedule schedule;

    void Start()
    {
        if (schedule != null)
        {
            foreach (EventorJob job in schedule.jobs)
            {
                if (job.GetType() == typeof(EventorAttackDescriptor))
                {
                    (job as EventorAttackDescriptor).animatedObject = animObj;
                    foreach (Damager d in (job as EventorAttackDescriptor).damageVolumes)
                        d.enabled = false;
                }
            }
        }
    }

    void OnGUI()
    {
        EventorGUI();
    }

    void EventorGUI()
    {
    }



    void AttackChainGUI()
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
                animObj.CrossFade(ad.clipProperties.clip.name, ad.clipProperties.blendTime, ad.clipProperties.playMode);
                while (animObj.IsPlaying(ad.clipProperties.clip.name))
                    yield return null;                            
        }
        isChainRunning = false;
    }


}
