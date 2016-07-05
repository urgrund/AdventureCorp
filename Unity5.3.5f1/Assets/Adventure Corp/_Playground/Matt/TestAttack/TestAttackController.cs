using UnityEngine;
using System.Collections;

public class TestAttackController : MonoBehaviour
{
    public Animation animObj;    

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
}
