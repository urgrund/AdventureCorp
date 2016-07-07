using UnityEngine;
using System.Collections;

public class TestAttackController : MonoBehaviour
{
    public Animation animObj;
    public Agent agent;
    public EventorSchedule[] schedules;
    public Damager[] damagers;


    void Start()
    {
        damagers = AttackVolumeCollection.CreateDamageCollidersForTransform(animObj.transform, agent.properties.GetComponent<AttackVolumeCollection>());
    }



    // Istantiate an EventorSchedule for use as an 
    // attack.   Setup needed properties and run!
    void AttackWithEventor(EventorSchedule attack)
    {
        attack = Instantiate(attack) as EventorSchedule;
        Helpers.ParentAndCenterOnTransform(attack.transform, this.transform);
        attack.isDestroyOnComplete = true;
        EventorAttackDescriptor attackDesc;
        for (int i = 0; i < attack.jobs.Count; i++)
        {
            if (attack.jobs[i].GetType() == typeof(EventorAttackDescriptor))
            {
                attackDesc = attack.jobs[i] as EventorAttackDescriptor;
                attackDesc.animatedObject = animObj;
                attackDesc.controller = this;
            }
        }
        attack.Run();
    }






    void OnGUI()
    {
        if (schedules == null)
            return;

        foreach (EventorSchedule s in schedules)
            if (GUILayout.Button(s.name))
                AttackWithEventor(s);
    }

    void OnDrawGizmos()
    {
        if (damagers == null)
            return;

        foreach (Damager d in damagers)
        {
            if (d.GetComponent<SphereCollider>())
            {                
                SphereCollider c = d.GetComponent<SphereCollider>();
                Gizmos.color = d.enabled ? Color.red : Color.grey;
                Gizmos.DrawWireSphere(d.transform.TransformPoint(c.center), c.radius);
                Gizmos.DrawCube(d.transform.TransformPoint(c.center), Vector3.one * c.radius * 0.5f);
            }
        }
    }
}
