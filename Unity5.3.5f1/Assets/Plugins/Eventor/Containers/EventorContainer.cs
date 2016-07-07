using UnityEngine;
using System.Collections;


/// <summary>
/// Container to hold a reference to an Event Schedule
/// When Run is called it will instantiate the Event at this components transform
/// </summary>
public class EventorContainer : MonoBehaviour
{
    public EventorSchedule schedule;
    public bool attachAsChild = true;
    
    // TODO - bother about this?
    //bool isRandomizeFromArray = false;
    //EventorSchedule[] schedules;


    private void RunSchedule(EventorSchedule e)
    {
        if (attachAsChild)
            EventorSchedule.RunAtTransformAsChild(e, transform);
        else
            EventorSchedule.RunAtLocation(e, transform);
    }

    public void Run()
    {
        RunSchedule(schedule);
    }

    //public EventorSchedule RunRandom()
    //{
    //    if (schedules != null)
    //    {
    //        if (schedules.Length > 0)
    //        {
    //            EventorSchedule e = schedules[Random.Range(0, schedules.Length)];
    //            if (e != null)
    //            {
    //                RunSchedule(e);
    //                return e;
    //            }
    //        }
    //    }
    //    return null;
    //}
}
