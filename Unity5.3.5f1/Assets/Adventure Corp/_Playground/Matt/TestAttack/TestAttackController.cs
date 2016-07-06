using UnityEngine;
using System.Collections;

public class TestAttackController : MonoBehaviour
{
    public Animation animObj;

    public EventorSchedule schedule;
    public Agent agent;

    public Damager[] damagers;

    void Start()
    {
        damagers = AttackVolumeCollection.CreateDamageCollidersForTransform(animObj.transform, agent.properties.GetComponent<AttackVolumeCollection>());

        schedule = Instantiate(schedule);
        Helpers.ParentAndCenterOnTransform(schedule.transform, transform);

        if (schedule != null)
        {
            foreach (EventorJob job in schedule.jobs)
            {
                if (job.GetType() == typeof(EventorAttackDescriptor))
                {
                    EventorAttackDescriptor attackDesc = job as EventorAttackDescriptor;
                    attackDesc.animatedObject = animObj;
                    attackDesc.attackManager = this;

                    if (attackDesc.attack == null)
                    {
                        Debug.LogError("No attack");
                        return;
                    }

                    //if (attackDesc.attack.volumes != null)
                    //{
                    //    foreach (Damager d in damagers)
                    //    {
                    //        foreach (AttackVolumeDescriptor avd in attackDesc.attack.volumes)
                    //            if (d.transform.parent.name == avd.boneName)
                    //                attackDesc.damagers.Add(d);
                    //    }
                    //}
                    //else
                    //    Debug.LogError("No volumes in this attack");

                }
            }
        }
    }
}
